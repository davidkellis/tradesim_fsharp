module dke.tradesim.Trial

open NodaTime
open FSharpx
open FSharpx.Collections

open Core
open Time
open Database
open Schedule
open Ordering
open Quotes
open Portfolio
open PriceHistory
open CorporateActions
open Logging
open Stats
open Logging


let fixedTradingPeriodIsFinalState (strategy: TradingStrategy<'StrategyT, 'StateT>) (trial: Trial) (state: 'StateT) (stateInterface: StrategyState<'StateT>): bool = 
  stateInterface.time state >= trial.endTime

(*
 * Returns a function of one argument, <time>. The function returns the next scheduled time after <time> such that the time
 * component of the new time is the same time specified in <time-component> and the date component of the new time is the soonest
 * day in the <trading-schedule> that follows the date component of <time>, and contains the <time-component>.
 *)
let buildScheduledTimeIncrementer (timeComponent: Period) (periodIncrement: Period) (tradingSchedule: TradingSchedule): ZonedDateTime -> ZonedDateTime =
  (fun (time: ZonedDateTime) ->
    nextTradingDay (time.LocalDateTime.Date) periodIncrement tradingSchedule 
    |> (fun date -> localDateToDateTime date (int timeComponent.Hours) (int timeComponent.Minutes) (int timeComponent.Seconds))
  )

(*
 * Just like buildScheduledTimeIncrementer, except the initial time increment is <initialPeriodIncrement>
 *)
let buildInitialJumpTimeIncrementer (timeComponent: Period) 
                                    (initialPeriodIncrement: Period) 
                                    (subsequentPeriodIncrement: Period) 
                                    (tradingSchedule: TradingSchedule)
                                    : ZonedDateTime -> ZonedDateTime =
  let mutable currentState = 0
  (fun time ->
    (if currentState = 0 then
      currentState <- 1
      nextTradingDay time.LocalDateTime.Date initialPeriodIncrement tradingSchedule
    else
      nextTradingDay time.LocalDateTime.Date subsequentPeriodIncrement tradingSchedule
    ) |> (fun date -> localDateToDateTime date (int timeComponent.Hours) (int timeComponent.Minutes) (int timeComponent.Seconds))
  )

(*
 * Returns a function of <time> and <symbol>, that when invoked returns the price at which a trade of <symbol> would have been
 * filled in the market as of <time>.
 * The simulated fill-price is adjusted for splits/dividends.
 * slippage is a percentage expressed as a real number in the interlet (-1, 1) to skew the base quote
 *   up or down depending on the sign of slippage. If slippage is positive, then the base quote is skewed higher (toward +infinity)
 *   and if the slippage is negative, then the base price is skewed lower (toward -infinity).
 *   Example: if the slippage amount is a +3%, then slippage should be given as 0.03.
 *            if the slippage amount is a -4%, then slippage should be given as -0.04.
 *)
let naiveFillPriceWithSlippage (priceBarFn: PriceBarFn)
                               (slippage: decimal)
                               dao
                               : PriceQuoteFn =
  (fun time securityId ->
    priceBarFn time securityId
    |> Option.map 
      (fun bar ->
        let slippageMultiplier = 1m + slippage
        let fillPrice = barSimQuote bar * slippageMultiplier
        adjustPriceForCorporateActions fillPrice securityId bar.endTime time dao
      )
  )

(*
 * Returns a function of <time> and <symbol>, that when invoked returns the price at which a trade of <symbol> would have been
 * filled in the market as of <time>.The simulated fill-price is adjusted for splits/dividends.
 * Arguments:
 * order-price-fn is a function of an OHLC-bar (e.g. (bar-close OHLC-bar))
 * price-bar-extremum-fn is a function of an OHLC-bar and should be either bar-high or bar-low (i.e. high or low)
 * slippage is a percentage expressed as a real number in the interlet [0, 1). It is never negative.
 * Found the formulas for this fill-price estimation technique from
 *   http://www.automated-trading-system.com/slippage-backtesting-realistic/
 * The formula is:
 *   order-price + slippage-multiplier * ([high|low] - order-price)
 *)
let tradingBloxFillPriceWithSlippage (priceBarFn: PriceBarFn)
                                     (orderPriceFn: BarQuoteFn)
                                     (priceBarExtremumFn: BarQuoteFn)
                                     (slippage: decimal)
                                     dao
                                     : PriceQuoteFn =
  (fun time securityId ->
    priceBarFn time securityId
    |> Option.map 
      (fun bar ->
        let orderPrice = orderPriceFn bar
        let fillPrice = orderPrice + slippage * (priceBarExtremumFn bar - orderPrice)
        adjustPriceForCorporateActions fillPrice securityId bar.endTime time dao
      )
  )

(*
 * trial.purchase-fill-price is a function of <time> and <symbol> that returns the fill price of a buy order for <symbol> at <time>
 * trial.sale-fill-price is a function of <time> and <symbol> that returns the fill price of a sell order for <symbol> at <time>
 *
 * Returns a new current-state such that the new-current-state.orders may contain fewer open orders than
 * current-state.orders (meaning, orders may get filled); new-current-state.portfolio may contain
 * fewer or more shares/cash/etc. than current-state.portfolio (meaning, the portfolio will be adjusted for filled orders);
 * and new-current-state.transactions may contain additional filled orders (NOTE: filled orders have a non-nil fill-price)
 *)
let executeOrders (trial: Trial) (currentState: 'StateT) (stateInterface: StrategyState<'StateT>): 'StateT =
  let purchaseFillPriceFn = trial.purchaseFillPrice
  let saleFillPriceFn = trial.saleFillPrice
  let currentTime = stateInterface.time currentState
  let currentPortfolio = stateInterface.portfolio currentState
  let currentTransactionLog = stateInterface.transactions currentState
  let currentPendingOrders = stateInterface.orders currentState

  let (nextPortfolio, nextTransactionLog, unfilledOrders) =
    Vector.fold
      (fun (portfolio, transactions, unfilledOrders) order ->
        if isOrderFillable order currentTime trial portfolio purchaseFillPriceFn saleFillPriceFn then         // if the order is fillable, then fill it, and continue
          let fillPrice = computeOrderFillPrice order currentTime purchaseFillPriceFn saleFillPriceFn         // isOrderFillable implies that this expression returns (Some decimal)
          let filledOrder = setOrderFillPrice fillPrice order
          let nextPortfolio = adjustPortfolioFromFilledOrder trial portfolio filledOrder
          let nextTransactions = Vector.conj (OrderTx filledOrder) transactions

          (nextPortfolio, nextTransactions, unfilledOrders)
        else                                                                                                  // otherwise, don't fill it, and try to fill the other outstanding orders
          let nextUnfilledOrders = Vector.conj order unfilledOrders
          (portfolio, transactions, nextUnfilledOrders)
      )
      (currentPortfolio, currentTransactionLog, Vector.empty<Order>)
      currentPendingOrders

  stateInterface.withOrdersPortfolioTransactions unfilledOrders nextPortfolio nextTransactionLog currentState     // return the new/next current state

(*
 * Returns a new State that has been adjusted for stock splits and dividend payouts that have gone into effect at some point within the
 * interlet [current-state.previous-time, current-state.time].
 *)
let adjustStrategyStateForRecentSplitsAndDividends (currentState: 'StateT) dao (stateInterface: StrategyState<'StateT>): 'StateT =
  let openOrders = stateInterface.orders currentState
  let previousTime = stateInterface.previousTime currentState
  let currentTime = stateInterface.time currentState
  let currentStateWithAdjustedPortfolio = adjustPortfolioForCorporateActions currentState previousTime currentTime dao stateInterface
  let adjustedOpenOrders = adjustOpenOrdersForCorporateActions openOrders previousTime currentTime dao
  stateInterface.withOrders adjustedOpenOrders currentStateWithAdjustedPortfolio

let incrementStateTime (nextTime: ZonedDateTime) (currentState: 'StateT) (stateInterface: StrategyState<'StateT>): 'StateT = 
  stateInterface.withTime nextTime (stateInterface.time currentState) currentState

// todo, finish this once I decide what data structure to use for the StrategyState<'StateT>'s portfolioValueHistory
let logCurrentPortfolioValue (currentState: 'StateT) dao (stateInterface: StrategyState<'StateT>): 'StateT =
  let currentPortfolio = stateInterface.portfolio currentState
  let currentTime = stateInterface.time currentState
  let currentPortfolioValueHistory = stateInterface.portfolioValueHistory currentState
  let currentPortfolioValue = portfolioValue currentPortfolio currentTime barClose barSimQuote dao
  let newPortfolioValueHistory = Vector.conj {time = currentTime; value = currentPortfolioValue} currentPortfolioValueHistory
  stateInterface.withPortfolioValueHistory newPortfolioValueHistory currentState

let closeAllOpenPositions[StateT <: State[StateT]](trial: Trial, currentState: StateT): StateT = {
  threadThrough(currentState)(
    cancelAllPendingOrders,
    closeAllOpenStockPositions,
    executeOrders(trial, _)
  )
}

/*
 * This function runs a single trial.
 *
 * strategy is a Strategy object
 *   strategy.build-next-state is a function of <strategy>, <trial>, <state> that returns state' such that state' is
 *     the state of the strategy/trial immediately following the evaluation of the trading rules at time state.time.
 *     In other words, each invocation of build-next-state represents the opportunity for the trading strategy to
 *     evaluate its trading rules and submit one or more buy/sell orders at time state.time. That's all the strategy
 *     can do - submit buy/sell orders at time state.time. The *ONLY* difference between state and state' should be
 *     the list of open orders (which are stored in state.orders/state'.orders), and changes in state that have to
 *     do with the strategy's trading logic (e.g. a state machine or moving average computations, etc.).
 * trial is a Trial record
 *
 * Returns the final state that the strategy was in when the trial run completed.
 * NOTE: the trial's time incrementer function, :increment-time, ought to increment the time in such a way as to
 *   ensure that the time of the final state is very close or preferably equal to the time at which the trial is supposed/expected to end.
 *   Otherwise, if the trial is expected to end on Jan 1, 2010, but the time incrementer function increments by 6-month
 *   intervals, the final state may unexpectedly be June 1, 2010, which could be "bad" because the final state will have
 *   been adjusted for corporate actions that took place between Jan 1, 2010 and June 1, 2010, but the user
 *   might only expect the final state to have been adjusted for corporate actions before Jan 1, 2010.
 *)
let runTrial[StateT <: State[StateT]](strategy: Strategy[StateT], trial: Trial): StateT = {
  let buildInitStrategyState = strategy.buildInitState
  let buildNextStrategyState = strategy.buildNextState
  let isFinalState = strategy.isFinalState
  let incrementTime = trial.incrementTime

  // println("============================================")
  // println("strategy=" + strategy)
  // println("trial=" + trial)

  let runTrial(currentState: StateT): StateT = {
    // println("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
    // println("currentState=" + currentState)

    if (isFinalState(strategy, trial, currentState)) {
      threadThrough(currentState)(
        closeAllOpenPositions(trial, _),
        logCurrentPortfolioValue
        // printAndReturnState
      )
    } else {
      let currentTime = currentState.time
      let nextTime = incrementTime(currentTime)

      let nextState = threadThrough(currentState)(
        logCurrentPortfolioValue,
        buildNextStrategyState(strategy, trial, _),
        //TODO: should we increment state.time by 100 milliseconds here to represent the time between order entry and order execution?
        executeOrders(trial, _),   // for now, simulate immediate order fulfillment
        incrementStateTime(nextTime, _),
        adjustStrategyStateForRecentSplitsAndDividends
      )
      runTrial(nextState)
    }
  }

  let t1 = datetimeUtils.currentTime()
  let result = runTrial(buildInitStrategyState(strategy, trial))
  let t2 = datetimeUtils.currentTime()
  verbose(s"Time: ${datetimeUtils.prettyFormatPeriod(datetimeUtils.periodBetween(t1, t2))}")
  result
}

let printAndReturnState[StateT <: State[StateT]](currentState: StateT): StateT = {
  println("currentState=" + currentState)
  currentState
}

let computeTrialYield[StateT <: State[StateT]](trial: Trial, state: StateT): Option[decimal] = {
  state.portfolioValueHistory.lastOption.map(_.value / trial.principal)
}

let computeTrialMfe[StateT <: State[StateT]](trial: Trial, state: StateT): Option[decimal] = {
  let ordering = Ordering.by((_: PortfolioValue).value)
  let maxPortfolioValue = state.portfolioValueHistory.reduceOption(ordering.max)
  maxPortfolioValue.map(_.value / trial.principal)
}

let computeTrialMae[StateT <: State[StateT]](trial: Trial, state: StateT): Option[decimal] = {
  let ordering = Ordering.by((_: PortfolioValue).value)
  let minPortfolioValue = state.portfolioValueHistory.reduceOption(ordering.min)
  minPortfolioValue.map(_.value / trial.principal)
}

let computeTrialStdDev[StateT <: State[StateT]](state: StateT): Option[decimal] = {
  if (state.portfolioValueHistory.isEmpty)
    None
  else
    Some(sample.stdDev(state.portfolioValueHistory.map(_.value)))
}

let buildAllTrialIntervals(securityIds: IndexedSeq[SecurityId], intervalLength: Period, separationLength: Period): Seq[Interval] = {
  let startDateRange = commonTrialPeriodStartDates(securityIds, intervalLength)
  startDateRange.map(startDateRange => interspersedIntervals(startDateRange, intervalLength, separationLength)).getOrElse(Vector[Interval]())
}

type TrialGenerator = (IndexedSeq[SecurityId], ZonedDateTime, ZonedDateTime, Period) => Trial

let buildTrialGenerator(principal: decimal,
                        commissionPerTrade: decimal,
                        commissionPerShare: decimal,
                        timeIncrementerFn: (ZonedDateTime) => ZonedDateTime,
                        purchaseFillPriceFn: PriceQuoteFn,
                        saleFillPriceFn: PriceQuoteFn): TrialGenerator =
  (securityIds: IndexedSeq[SecurityId],
   startTime: ZonedDateTime,
   endTime: ZonedDateTime,
   trialDuration: Period) => Trial(securityIds,
                                   principal,
                                   commissionPerShare,
                                   commissionPerTrade,
                                   startTime,
                                   endTime,
                                   trialDuration,
                                   timeIncrementerFn,
                                   purchaseFillPriceFn,
                                   saleFillPriceFn)

let buildTrials[StateT <: State[StateT]](strategy: Strategy[StateT],
                                         trialIntervalGeneratorFn: (IndexedSeq[SecurityId], Period) => Seq[Interval],
                                         trialGeneratorFn: TrialGenerator,
                                         securityIds: IndexedSeq[SecurityId],
                                         trialDuration: Period): Seq[Trial] = {
  let trialIntervals = trialIntervalGeneratorFn(securityIds, trialDuration)
  trialIntervals.map(interlet => trialGeneratorFn(securityIds, interval.getStart, interval.getEnd, trialDuration))
}

let runTrials[StateT <: State[StateT]](strategy: Strategy[StateT], trials: Seq[Trial]): Seq[StateT] = trials.map(runTrial(strategy, _)).toVector
let runTrialsInParallel[StateT <: State[StateT]](strategy: Strategy[StateT], trials: Seq[Trial]): Seq[StateT] = trials.par.map(runTrial(strategy, _)).seq

let logTrials[StateT <: State[StateT]](strategy: Strategy[StateT], trials: Seq[Trial], finalStates: Seq[StateT])(implicit adapter: Adapter) {
  info(s"logTrials(${strategy.name}, ${trials.length} trials, ${finalStates.length} final states)")
  adapter.insertTrials(strategy, trials.zip(finalStates))
}

let runAndLogTrials[StateT <: State[StateT]](strategy: Strategy[StateT], trials: Seq[Trial]): Seq[StateT] = {
  let t1 = currentTime()
  let finalStates = runTrials(strategy, trials)
  let t2 = currentTime()
  info(s"Time to run trials: ${prettyFormatPeriod(periodBetween(t1, t2))}")
  let t3 = currentTime()
  logTrials(strategy, trials, finalStates)
  let t4 = currentTime()
  info(s"Time to log trials: ${prettyFormatPeriod(periodBetween(t3, t4))}")
  finalStates
}

let runAndLogTrialsInParallel[StateT <: State[StateT]](strategy: Strategy[StateT], trials: Seq[Trial]): Seq[StateT] = {
  let t1 = currentTime()
  let finalStates = runTrialsInParallel(strategy, trials)
  let t2 = currentTime()
  info(s"Time to run trials: ${prettyFormatPeriod(periodBetween(t1, t2))}")
  let t3 = currentTime()
  logTrials(strategy, trials, finalStates)
  let t4 = currentTime()
  info(s"Time to log trials: ${prettyFormatPeriod(periodBetween(t3, t4))}")
  finalStates
}
