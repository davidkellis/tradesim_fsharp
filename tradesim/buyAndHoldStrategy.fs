module dke.tradesim.strategies.BuyAndHold

open System.Collections.Immutable
open FSharpx
open FSharpx.Collections
open NodaTime

open dke.tradesim.AdjustedQuotes
open dke.tradesim.Core
open dke.tradesim.Time
open dke.tradesim.Logging
open dke.tradesim.Math
open dke.tradesim.Ordering
open dke.tradesim.Quotes
open dke.tradesim.Schedule
open dke.tradesim.Securities
open dke.tradesim.Trial

type BuyAndHoldState = {
  previousTime: ZonedDateTime
  time: ZonedDateTime
  portfolio: Portfolio
  orders: Vector<Order>
  transactions: TransactionLog
  portfolioValueHistory: Vector<PortfolioValue>

  hasEnteredPosition: bool
}

type BuyAndHoldStrategy = {
  name: string
  buildInitialState: BuyAndHoldStrategy -> Trial -> BuyAndHoldState
  buildNextState: BuyAndHoldStrategy -> Trial -> BuyAndHoldState -> BuyAndHoldState
  isFinalState: BuyAndHoldStrategy -> Trial -> BuyAndHoldState -> bool
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

let initialState (strategy: TradingStrategy<BuyAndHoldStrategy, BuyAndHoldState>) (trial: Trial): BuyAndHoldState = StrategyStateImpl.initialize trial.startTime trial.principal

let nextState (strategy: TradingStrategy<BuyAndHoldStrategy, BuyAndHoldState>) (trial: Trial) (state: BuyAndHoldState) dao: BuyAndHoldState =
  let time = state.time
  let endTime = trial.endTime
  let securityId = trial.securityIds.head
  let portfolio = state.portfolio

  if not state.hasEnteredPosition then
    let qty = maxSharesPurchasable trial portfolio.cash time securityId (adjEodSimQuote dao) |> Option.getOrElse 0M |> Decimal.floor |> int64
    let qtyToBuy = if qty > 1 then qty - 1 else qty      // we're conservative with how many shares we purchase so we don't have to buy on margin if the price unexpectedly goes up
    let newState = buy StrategyStateImpl state time securityId qty
    {newState with hasEnteredPosition = true}
  elif time = endTime then
//      info(s"sell all shares")
    sell StrategyStateImpl state time securityId <| sharesOnHand portfolio securityId
  else
    state

//type TradingStrategy<'StrategyT, 'StateT> = {
//  name: 'StrategyT -> string
//  buildInitialState: 'StrategyT -> ('StrategyT -> Trial -> 'StateT)
//  buildNextState: 'StrategyT -> ('StrategyT -> Trial -> 'StateT -> 'StateT)
//  isFinalState: 'StrategyT -> ('StrategyT -> Trial -> 'StateT -> bool)
//}
let TradingStrategyImpl: TradingStrategy<BuyAndHoldStrategy, BuyAndHoldState> = {
  name = fun strategy -> strategy.name
  buildInitialState = fun strategy -> strategy.buildInitialState
  buildNextState = fun strategy -> strategy.buildNextState
  isFinalState = fun strategy -> strategy.isFinalState
}

let buildStrategy unit: BuyAndHoldStrategy = {
  name = "Buy And Hold"
  buildInitialState = initialState
  buildNextState = nextState
  isFinalState = fixedTradingPeriodIsFinalState StrategyStateImpl
}

module Scenarios =
  let runSingleTrial1 dao: unit =
    let trialDuration = years 1L
    let startTime = datetime 2003 2 15 12 0 0
    let endTime = startTime + trialDuration.ToDuration()
    let tradingSchedule = buildTradingSchedule defaultTradingSchedule defaultHolidaySchedule
    let timeIncrementerFn = buildScheduledTimeIncrementer (hours 12) (days 1) tradingSchedule
//      let timeIncrementerFn = buildInitialJumpTimeIncrementer(new LocalTime(12, 0, 0), periodBetween(startTime, endTime), days(1), tradingSchedule)
    let purchaseFillPriceFn = tradingBloxFillPriceWithSlippage findEodBar barSimQuote barHigh 0.3 dao
    let saleFillPriceFn = tradingBloxFillPriceWithSlippage findEodBar barSimQuote barLow 0.3 dao
    let strategy = buildStrategy ()
    let securityIds = findSecurities (PrimaryUsExchanges dao) ["AAPL"] dao |> Seq.flatMap (fun security -> security.id) Vector.ofSeq
    let trial: Trial = {
      securityIds = securityIds
      principal = 10000
      commissionPerTrade = 7M
      commissionPerShare = 0M
      startTime = startTime
      endTime = endTime
      duration = trialDuration
      incrementTime = timeIncrementerFn
      purchaseFillPrice = purchaseFillPriceFn
      saleFillPrice = saleFillPriceFn
    }
    info "Running 1 trial"
    runAndLogTrialsInParallel TradingStrategyImpl StrategyStateImpl strategy [trial] dao |> ignore
