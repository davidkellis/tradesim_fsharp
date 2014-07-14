module dke.tradesim.Trial

open NodaTime
open FSharpx
open FSharpx.Collections
open FSharp.Collections.ParallelSeq

open Stdlib
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


let fixedTradingPeriodIsFinalState (strategy: TradingStrategy<'StrategyT, 'StateT>) (trial: Trial) (stateInterface: StrategyState<'StateT>) (state: 'StateT): bool = 
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
  let currentState = ref 0
  (fun time ->
    (if !currentState = 0 then
      currentState := 1
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
let executeOrders (trial: Trial) (stateInterface: StrategyState<'StateT>) (currentState: 'StateT): 'StateT =
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
let adjustStrategyStateForRecentSplitsAndDividends (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) dao: 'StateT =
  let openOrders = stateInterface.orders currentState
  let previousTime = stateInterface.previousTime currentState
  let currentTime = stateInterface.time currentState
  let currentStateWithAdjustedPortfolio = adjustPortfolioForCorporateActions stateInterface currentState previousTime currentTime dao
  let adjustedOpenOrders = adjustOpenOrdersForCorporateActions openOrders previousTime currentTime dao
  stateInterface.withOrders adjustedOpenOrders currentStateWithAdjustedPortfolio

let incrementStateTime (nextTime: ZonedDateTime) (stateInterface: StrategyState<'StateT>) (currentState: 'StateT): 'StateT = 
  stateInterface.withTime nextTime (stateInterface.time currentState) currentState

// todo, finish this once I decide what data structure to use for the StrategyState<'StateT>'s portfolioValueHistory
let logCurrentPortfolioValue (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) dao: 'StateT =
  let currentPortfolio = stateInterface.portfolio currentState
  let currentTime = stateInterface.time currentState
  let currentPortfolioValueHistory = stateInterface.portfolioValueHistory currentState
  let currentPortfolioValue = portfolioValue currentPortfolio currentTime barClose barSimQuote dao
  let newPortfolioValueHistory = Vector.conj {time = currentTime; value = currentPortfolioValue} currentPortfolioValueHistory
  stateInterface.withPortfolioValueHistory newPortfolioValueHistory currentState

let closeAllOpenPositions (trial: Trial) (stateInterface: StrategyState<'StateT>) (currentState: 'StateT): 'StateT =
  currentState
  |> cancelAllPendingOrders stateInterface
  |> closeAllOpenStockPositions stateInterface
  |> executeOrders trial stateInterface

let printAndReturnState (stateInterface: StrategyState<'StateT>) (currentState: 'StateT): 'StateT =
  printfn "currentState=%s" (stateInterface.toString currentState)
  currentState

(*
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
let runTrial (strategyInterface: TradingStrategy<'StrategyT, 'StateT>) (stateInterface: StrategyState<'StateT>) (strategy: 'StrategyT) (trial: Trial) dao: 'StateT =
  let buildInitStrategyState = strategyInterface.buildInitialState strategy
  let buildNextStrategyState = strategyInterface.buildNextState strategy
  let isFinalState = strategyInterface.isFinalState strategy
  let incrementTime = trial.incrementTime

  // println("============================================")
  // println("strategy=" + strategy)
  // println("trial=" + trial)

  let rec runTrialR (currentState: 'StateT): 'StateT =
    // println("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")
    // println("currentState=" + currentState)

    if isFinalState strategy trial currentState then
      currentState
      |> closeAllOpenPositions trial stateInterface
      |> fun state -> logCurrentPortfolioValue stateInterface state dao
//      |> printAndReturnState
    else
      let currentTime = stateInterface.time currentState
      let nextTime = incrementTime currentTime

      let nextState = 
        currentState
        |> fun state -> logCurrentPortfolioValue stateInterface state dao
        |> buildNextStrategyState strategy trial
        //todo: should we increment state.time by 100 milliseconds here to represent the time between order entry and order execution?
        |> executeOrders trial stateInterface   // for now, simulate immediate order fulfillment
        |> incrementStateTime nextTime stateInterface
        |> fun state -> adjustStrategyStateForRecentSplitsAndDividends stateInterface state dao

      runTrialR nextState

  let t1 = currentTime <| Some EasternTimeZone
  let result = runTrialR <| buildInitStrategyState strategy trial
  let t2 = currentTime <| Some EasternTimeZone
  verbose <| sprintf "Time: ${datetimeUtils.prettyFormatPeriod(datetimeUtils.periodBetween(t1, t2))}"
  result

let computeTrialYield (trial: Trial) (stateInterface: StrategyState<'StateT>) (state: 'StateT): Option<decimal> =
  stateInterface.portfolioValueHistory state
  |> Vector.tryLast
  |> Option.map (fun pv -> pv.value / trial.principal)

let computeTrialMfe (trial: Trial) (stateInterface: StrategyState<'StateT>) (state: 'StateT): Option<decimal> =
  (stateInterface.portfolioValueHistory state)
  |> Seq.reduceOption (maxBy (fun pv -> pv.value))
  |> Option.map (fun pv -> pv.value / trial.principal)

let computeTrialMae (trial: Trial) (stateInterface: StrategyState<'StateT>) (state: 'StateT): Option<decimal> =
  (stateInterface.portfolioValueHistory state)
  |> Seq.reduceOption (minBy (fun pv -> pv.value))
  |> Option.map (fun pv -> pv.value / trial.principal)

let computeTrialStdDev (stateInterface: StrategyState<'StateT>) (state: 'StateT): Option<decimal> =
  let portfolioValueHistory = stateInterface.portfolioValueHistory state
  if Seq.isEmpty portfolioValueHistory then
    None
  else
    Some <| Sample.stdDev (Seq.map (fun pv -> pv.value) portfolioValueHistory)

let buildAllTrialIntervals (securityIds: Vector<SecurityId>) (intervalLength: Period) (separationLength: Period) dao: seq<Interval> =
  commonTrialPeriodStartDates securityIds intervalLength dao
  |> Option.map (fun startDateRange -> interspersedIntervals startDateRange intervalLength separationLength)
  |> Option.getOrElse Seq.empty

// TrialGenerator = (securityIds: Vector<SecurityId>) (startTime: ZonedDateTime) (endTime: ZonedDateTime) (trialDuration: Period) -> Trial
type TrialGenerator = Vector<SecurityId> -> ZonedDateTime -> ZonedDateTime -> Period -> Trial

let buildTrialGenerator (principal: decimal)
                        (commissionPerTrade: decimal)
                        (commissionPerShare: decimal)
                        (timeIncrementerFn: ZonedDateTime -> ZonedDateTime)
                        (purchaseFillPriceFn: PriceQuoteFn)
                        (saleFillPriceFn: PriceQuoteFn)
                        : TrialGenerator =
  (fun (securityIds: Vector<SecurityId>) (startTime: ZonedDateTime) (endTime: ZonedDateTime) (trialDuration: Period) -> 
    {
      securityIds = securityIds
      principal = principal
      commissionPerTrade = commissionPerTrade
      commissionPerShare = commissionPerShare
      startTime = startTime
      endTime = endTime
      duration = trialDuration
      incrementTime = timeIncrementerFn
      purchaseFillPrice = purchaseFillPriceFn
      saleFillPrice = saleFillPriceFn
    }
  )

let buildTrials (trialIntervalGeneratorFn: Vector<SecurityId> -> Period -> seq<Interval>)
                (trialGeneratorFn: TrialGenerator)
                (securityIds: Vector<SecurityId>)
                (trialDuration: Period)
                : seq<Trial> =
  trialIntervalGeneratorFn securityIds trialDuration
  |> Seq.map
    (fun interval -> trialGeneratorFn securityIds (interval.Start |> instantToEasternTime) (interval.End |> instantToEasternTime) trialDuration)

let runTrials strategyInterface stateInterface (strategy: 'StrategyT) (trials: seq<Trial>) dao: seq<'StateT> = 
  Seq.map (fun trial -> runTrial strategyInterface stateInterface strategy trial dao) trials

let runTrialsInParallel strategyInterface stateInterface (strategy: 'StrategyT) (trials: seq<Trial>) dao: seq<'StateT> = 
  trials
  |> PSeq.map (fun trial -> runTrial strategyInterface stateInterface strategy trial dao)
  |> PSeq.toArray
  |> Array.toSeq

let logTrials (strategyInterface: TradingStrategy<'StrategyT, 'StateT>) (strategy: 'StrategyT) (trials: seq<Trial>) (finalStates: seq<'StateT>) dap: unit =
  infoL <| lazy ( sprintf "logTrials -> strategy=%s, %i trials, %i final states" (strategyInterface.name strategy) (Seq.length trials) (Seq.length finalStates))
  dao.insertTrials strategy <| Seq.zip trials finalStates

let runAndLogTrials (strategyInterface: TradingStrategy<'StrategyT, 'StateT>) (stateInterface: StrategyState<'StateT>) (strategy: 'StrategyT) (trials: seq<Trial>) dao: seq<'StateT> =
  let t1 = currentTime <| Some EasternTimeZone
  let finalStates = runTrials strategyInterface stateInterface strategy trials dao
  let t2 = currentTime <| Some EasternTimeZone
  info <| sprintf "Time to run trials: %s" (prettyFormatPeriod <| periodBetween t1 t2)
  let t3 = currentTime <| Some EasternTimeZone
  logTrials strategyInterface strategy trials finalStates dao
  let t4 = currentTime <| Some EasternTimeZone
  info <| sprintf "Time to log trials: %s" (prettyFormatPeriod <| periodBetween t3 t4)
  finalStates

let runAndLogTrialsInParallel (strategyInterface: TradingStrategy<'StrategyT, 'StateT>) (stateInterface: StrategyState<'StateT>) (strategy: 'StrategyT) (trials: seq<Trial>) dao: seq<'StateT> =
  let t1 = currentTime <| Some EasternTimeZone
  let finalStates = runTrialsInParallel strategyInterface stateInterface strategy trials dao
  let t2 = currentTime <| Some EasternTimeZone
  info <| sprintf "Time to run trials: %s" (prettyFormatPeriod <| periodBetween t1 t2)
  let t3 = currentTime <| Some EasternTimeZone
  logTrials strategyInterface strategy trials finalStates dao
  let t4 = currentTime <| Some EasternTimeZone
  info <| sprintf "Time to log trials: %s" (prettyFormatPeriod <| periodBetween t3 t4)
  finalStates
