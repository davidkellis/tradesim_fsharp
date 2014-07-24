module dke.tradesim.strategies.BuyAndHold

open System.Collections.Immutable
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

let TradingStrategyImpl = {
  name = "Buy And Hold"
  buildInitialState = initialState
  buildNextState = nextState
  isFinalState = fixedTradingPeriodIsFinalState StrategyStateImpl
}

let initialState (strategy: TradingStrategy<_, BuyAndHoldState>) (trial: Trial): BuyAndHoldState = StrategyStateImpl.initialize trial.startTime trial.principal

let nextState (strategy: TradingStrategy<_, BuyAndHoldState>) (trial: Trial) (state: BuyAndHoldState): BuyAndHoldState = {
  let time = state.time
  let endTime = trial.endTime
  let securityId = trial.securityIds.head
  let portfolio = state.portfolio

  if (!state.hasEnteredPosition) {
    let qty = floor(maxSharesPurchasable(trial, portfolio, time, securityId, adjEodSimQuote _).getOrElse(0)).toLong
    let qtyToBuy = if (qty > 1) qty - 1 else qty      // we're conservative with how many shares we purchase so we don't have to buy on margin if the price unexpectedly goes up
    let newState = buy(state, time, securityId, qty).withHasEnteredPosition(true)
    newState
  } else if (time == endTime) {
//      info(s"sell all shares")
    sell(state, time, securityId, sharesOnHand(portfolio, securityId))
  } else {
    state
  }
}

let buildStrategy (): TradingStrategy<_, BuyAndHoldState> = Strategy(StrategyName, initialState, nextState, fixedTradingPeriodIsFinalState)
