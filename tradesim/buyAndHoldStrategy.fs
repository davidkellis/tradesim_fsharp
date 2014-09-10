module dke.tradesim.strategies.BuyAndHold

open System.Collections.Immutable
open FSharpx
open FSharpx.Collections
open NodaTime

open dke.tradesim
open dke.tradesim.AdjustedQuotes
open dke.tradesim.Core
open dke.tradesim.Database
open dke.tradesim.Time
open dke.tradesim.Logging
open dke.tradesim.Math
open dke.tradesim.Ordering
open dke.tradesim.Quotes
open dke.tradesim.Schedule
open dke.tradesim.Securities
open dke.tradesim.Trial

type State = {
  previousTime: ZonedDateTime
  time: ZonedDateTime
  portfolio: Portfolio
  orders: Vector<Order>
  transactions: TransactionLog
  portfolioValueHistory: Vector<PortfolioValue>

  hasEnteredPosition: bool
}

type Strategy = {
  name: string
  buildInitialState: Strategy -> Trial -> State
  buildNextState: Strategy -> Trial -> State -> State
  isFinalState: Strategy -> Trial -> State -> bool
}

let StrategyStateImpl = {
  previousTime = fun state -> state.previousTime
  time = fun state -> state.time
  portfolio = fun state -> state.portfolio
  orders = fun state -> state.orders
  transactions = fun state -> state.transactions
  portfolioValueHistory = fun state -> state.portfolioValueHistory

  // (time: ZonedDateTime) -> (principal: decimal) -> 'StateT
  initialize = fun time principal -> 
    {
      previousTime = time
      time = time
      portfolio = createPortfolio principal
      orders = Vector.empty
      transactions = Vector.empty
      portfolioValueHistory = Vector.empty

      hasEnteredPosition = false
    }

  withTime = fun newTime newPreviousTime oldState -> {oldState with time = newTime; previousTime = newPreviousTime}
  withOrders = fun newOrders oldState -> {oldState with orders = newOrders}
  withPortfolio = fun newPortfolio oldState -> {oldState with portfolio = newPortfolio}
  withTransactions = fun newTransactionLog oldState -> {oldState with transactions = newTransactionLog}
  withPortfolioValueHistory = fun newPortfolioValueHistory oldState -> {oldState with portfolioValueHistory = newPortfolioValueHistory}

  withOrdersPortfolioTransactions = fun newOrders newPortfolio newTransactionLog oldState -> {oldState with orders = newOrders; portfolio = newPortfolio; transactions = newTransactionLog}

  toString = fun state -> 
    sprintf "BAH: pt=%i t=%i pflio=[cash=%M stocks=%s] orders=%s hasEnteredPosition=%b" 
      <| dateTimeToTimestamp state.previousTime
      <| dateTimeToTimestamp state.time
      <| state.portfolio.cash
      <| state.portfolio.stocks.ToString()
      <| state.orders.ToString()
      <| state.hasEnteredPosition
}

let initialState (strategy: Strategy) (trial: Trial): State = StrategyStateImpl.initialize trial.startTime trial.principal

let nextState dao (strategy: Strategy) (trial: Trial) (state: State): State =
  let time = state.time
  let endTime = trial.endTime
  let securityId = trial.securityIds |> Seq.head
  let portfolio = state.portfolio

  if not state.hasEnteredPosition then
    let qty = maxSharesPurchasable trial portfolio.cash time securityId (adjEodSimQuote dao) |> Option.getOrElse 0M |> Decimal.floor |> int64
    let qtyToBuy = if qty > 1L then qty - 1L else qty      // we're conservative with how many shares we purchase so we don't have to buy on margin if the price unexpectedly goes up
    let newState = buy StrategyStateImpl state time securityId qty
    {newState with hasEnteredPosition = true}
  elif time = endTime then
//      info(s"sell all shares")
    sell StrategyStateImpl state time securityId <| sharesOnHand portfolio securityId
  else
    state

let TradingStrategyImpl: TradingStrategy<Strategy, State> = {
  name = fun strategy -> strategy.name
  buildInitialState = fun strategy -> strategy.buildInitialState
  buildNextState = fun strategy -> strategy.buildNextState
  isFinalState = fun strategy -> strategy.isFinalState
}

let buildStrategy dao: Strategy = {
  name = "Buy And Hold"
  buildInitialState = initialState
  buildNextState = nextState dao
  isFinalState = fixedTradingPeriodIsFinalState TradingStrategyImpl StrategyStateImpl
}

let buildDefaultBuyAndHoldStrategyAndTrials dao tickerSymbols principal commissionPerTrade commissionPerShare trialPeriodLength: Strategy * seq<Trial> =
  let tradingSchedule = buildTradingSchedule defaultTradingSchedule defaultHolidaySchedule
  let timeIncrementerFn = buildScheduledTimeIncrementer (hours 12L) (days 1L) tradingSchedule
  let purchaseFillPriceFn = tradingBloxFillPriceWithSlippage dao (findEodBar dao) barSimQuote barHigh 0.3M
  let saleFillPriceFn = tradingBloxFillPriceWithSlippage dao (findEodBar dao) barSimQuote barLow 0.3M
  let strategy = buildStrategy dao
  let exchanges = PrimaryUsExchanges dao
  let securityIds = findSecurities dao exchanges tickerSymbols |> Seq.flatMapO (fun security -> security.id) |> Vector.ofSeq
  let trialGenerator = buildTrialGenerator principal commissionPerTrade commissionPerShare timeIncrementerFn purchaseFillPriceFn saleFillPriceFn
  let trialIntervalBuilderFn = 
    fun securityIds trialPeriodLength -> 
      buildAllTrialIntervals dao (days 1L) securityIds trialPeriodLength 
      |> Seq.filter (fun interval -> isTradingDay tradingSchedule (interval.Start |> instantToEasternTime |> dateTimeToLocalDate) )
  let trials = buildTrials trialIntervalBuilderFn trialGenerator securityIds trialPeriodLength
  (strategy, trials)


module Scenarios =
  let runSingleTrial1 dao: unit =
    let trialPeriodLength = years 1L
    let startTime = datetime 2003 2 15 12 0 0
    let endTime = (startTime.LocalDateTime + trialPeriodLength).InZoneLeniently(EasternTimeZone)
    let tradingSchedule = buildTradingSchedule defaultTradingSchedule defaultHolidaySchedule
    let timeIncrementerFn = buildScheduledTimeIncrementer (hours 12L) (days 1L) tradingSchedule
//    let timeIncrementerFn = buildInitialJumpTimeIncrementer (hours 12L) (periodBetween startTime endTime) (days 1L) tradingSchedule
//    let purchaseFillPriceFn = tradingBloxFillPriceWithSlippage dao (findEodBar dao) barSimQuote barHigh 0.3M
//    let saleFillPriceFn = tradingBloxFillPriceWithSlippage dao (findEodBar dao) barSimQuote barLow 0.3M
    let purchaseFillPriceFn = naiveFillPrice dao (findEodBar dao) barSimQuote
    let saleFillPriceFn = naiveFillPrice dao (findEodBar dao) barSimQuote
    let strategy = buildStrategy dao
    let exchanges = PrimaryUsExchanges dao
    let securityIds = findSecurities dao exchanges ["AAPL"] |> Seq.flatMapO (fun security -> security.id) |> Vector.ofSeq
    let trial: Trial = {
      securityIds = securityIds
      principal = 10000M
      commissionPerTrade = 7M
      commissionPerShare = 0M
      startTime = startTime
      endTime = endTime
      duration = trialPeriodLength
      incrementTime = timeIncrementerFn
      purchaseFillPrice = purchaseFillPriceFn
      saleFillPrice = saleFillPriceFn
    }
    info "Running 1 trial"
    runAndLogTrials TradingStrategyImpl StrategyStateImpl dao strategy [trial] |> ignore

  let runMultipleTrials1 dao: unit =
    let (strategy, trials) = buildDefaultBuyAndHoldStrategyAndTrials dao ["AAPL"] 10000M 7M 0M <| years 1L
    info <| sprintf "Running %i trials" (Seq.length trials)
    let (finalStates, trialResults) = runAndLogTrialsInParallel TradingStrategyImpl StrategyStateImpl dao strategy trials
    TrialSetStats.printReport trialResults

  let compareMutualFunds dao: unit =
    let tradingSchedule = buildTradingSchedule defaultTradingSchedule defaultHolidaySchedule
    let timeIncrementerFn = buildScheduledTimeIncrementer (hours 12L) (days 1L) tradingSchedule
    let purchaseFillPriceFn = tradingBloxFillPriceWithSlippage dao (findEodBar dao) barSimQuote barHigh 0.3M
    let saleFillPriceFn = tradingBloxFillPriceWithSlippage dao (findEodBar dao) barSimQuote barLow 0.3M
    let strategy = buildStrategy dao
    let exchanges = PrimaryUsExchanges dao
    let tickerSymbols = [
      "MIDHX";
      "RERCX";
      "ODVNX";
      "OIBNX";
      "WMGRX";
      "SAMVX";
      "TRLGX";
      "PSSMX";
      "PLFMX";
      "CMPIX";
      "PRRRX";
      "PTRRX";
      "FSIAX"
    ]
    let securities = findSecurities dao exchanges tickerSymbols
    let trialGenerator = buildTrialGenerator 10000M 7M 0M timeIncrementerFn purchaseFillPriceFn saleFillPriceFn
    let trialIntervalBuilderFn = 
      fun securityIds trialPeriodLength -> 
        buildAllTrialIntervals dao (days 1L) securityIds trialPeriodLength 
        |> Seq.filter (fun interval -> isTradingDay tradingSchedule (interval.Start |> instantToEasternTime |> dateTimeToLocalDate) )
    let trialPeriodLength = years 1L

    securities
    |> Seq.iter
      (fun security ->
        let securityIds = [security.id |> Option.get] |> Vector.ofSeq
        info "Building trials"
        let trials = buildTrials trialIntervalBuilderFn trialGenerator securityIds trialPeriodLength
        info <| sprintf "Running %i trials" (Seq.length trials)
        let (finalStates, trialResults) = runAndLogTrialsInParallel TradingStrategyImpl StrategyStateImpl dao strategy trials
        TrialSetStats.printReport trialResults
      )
