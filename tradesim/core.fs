module dke.tradesim.Core

open System.Collections.Immutable
open FSharpx.Collections
open NodaTime

open Stdlib
open Time
open Stats

type SecurityId = int
type SecurityIds = seq<SecurityId>

type MarketOrderDetails = {
  time: ZonedDateTime
  securityId: SecurityId
  qty: int64
  fillPrice: Option<decimal>
}

type LimitOrderDetails = {
  time: ZonedDateTime
  securityId: SecurityId
  qty: int64
  limitPrice: decimal
  fillPrice: Option<decimal>
}

type Order =
  MarketBuy of MarketOrderDetails
  | MarketSell of MarketOrderDetails
  | LimitBuy of LimitOrderDetails
  | LimitSell of LimitOrderDetails

let limitToMarketOrderDetails (limitDetails: LimitOrderDetails): MarketOrderDetails = {
  time = limitDetails.time
  securityId = limitDetails.securityId
  qty = limitDetails.qty
  fillPrice = limitDetails.fillPrice
}

type StockHoldings = ImmutableDictionary<SecurityId, int64>
type Portfolio = {
  cash: decimal
  stocks: StockHoldings
}

let createPortfolio principal = {cash = principal; stocks = ImmutableDictionary.Empty}

type PortfolioValue = {
  time: ZonedDateTime
  value: decimal
}
type PortfolioValues = seq<PortfolioValue>

type PriceQuoteFn = ZonedDateTime -> SecurityId -> Option<decimal>

type Trial = {
  securityIds: seq<SecurityId>
  principal: decimal
  commissionPerTrade: decimal
  commissionPerShare: decimal
  startTime: ZonedDateTime
  endTime: ZonedDateTime
  duration: Period
  incrementTime: ZonedDateTime -> ZonedDateTime
  purchaseFillPrice: PriceQuoteFn
  saleFillPrice: PriceQuoteFn
}

type TrialAttribute = TrialYield | Mfe | Mae | StdDev


// core domain concepts - exchange, industry, sector, security, bar

type Exchange = {
  id: Option<int>
  label: string
  name: Option<string>
}

type Industry = { name: string }
type Sector = { name: string }

type Security = {
  id: Option<int>
  bbGid: string
  bbGcid: string
  kind: string
  symbol: string
  name: string
  startDate: Option<LocalDate>
  endDate: Option<LocalDate>
  cik: Option<int>
  active: Option<bool>
  fiscalYearEndDate: Option<int>
  exchangeId: Option<int>
  industryId: Option<int>
  sectorId: Option<int>
}

type Bar = {
  id: Option<int>
  securityId: SecurityId
  startTime: ZonedDateTime
  endTime: ZonedDateTime
  o: decimal
  h: decimal
  l: decimal
  c: decimal
  volume: int64
}

let barOpen bar = bar.o
let barHigh bar = bar.h
let barLow bar = bar.l
let barClose bar = bar.c

type PriceBarFn = ZonedDateTime -> SecurityId -> Option<Bar>


// corporate actions

type Split = {
  securityId: SecurityId
  exDate: LocalDate
  ratio: decimal
}

// See http://www.investopedia.com/articles/02/110802.asp#axzz24Wa9LgDj for the various dates associated with dividend payments
// See also http://www.sec.gov/answers/dividen.htm
type CashDividend = {
  securityId: SecurityId
  declarationDate: Option<LocalDate>    // date at which the announcement to shareholders/market that company will pay a dividend is made
  exDate: LocalDate                     // on or after this date, the security trades without the dividend
  recordDate: Option<LocalDate>         // date at which shareholders of record are identified as recipients of the dividend
  payableDate: Option<LocalDate>        // date at which company issues payment of dividend
  amount: decimal
}

type CorporateAction =
  SplitCA of Split
  | CashDividendCA of CashDividend

let corporateActionSecurityId = function
  | SplitCA split -> split.securityId
  | CashDividendCA dividend -> dividend.securityId

let corporateActionExDate = function
  | SplitCA split -> split.exDate
  | CashDividendCA dividend -> dividend.exDate


// quarterly and annual report types

type StatementAttributeValue =
  HeaderAttribute of string
  | StringAttribute of string
  | NumericAttribute of decimal
type Statement = Map<string, StatementAttributeValue>

type QuarterlyReport = {
  securityId: SecurityId
  startTime: ZonedDateTime
  endTime: ZonedDateTime
  publicationTime: ZonedDateTime
  incomeStatement: Statement
  balanceSheet: Statement
  cashFlowStatement: Statement
}

type AnnualReport = {
  securityId: SecurityId
  startTime: ZonedDateTime
  endTime: ZonedDateTime
  publicationTime: ZonedDateTime
  incomeStatement: Statement
  balanceSheet: Statement
  cashFlowStatement: Statement
}


// transaction details

type SplitAdjustment = {
  securityId: SecurityId
  exDate: LocalDate
  ratio: decimal
  adjustmentTime: ZonedDateTime
  shareQtyDelta: int64
  cashPayout: decimal
}

type CashDividendPayment = {
  securityId: SecurityId
  exDate: LocalDate                     // on or after this date, the security trades without the dividend
  payableDate: Option<LocalDate>        // date at which company issues payment of dividend
  amountPerShare: decimal               // amount of the dividend, per share
  adjustmentTime: ZonedDateTime         // time at which the adjustment took place
  shareQty: int64                       // number of shares on hand of <securityId>
  total: decimal
}

type Transaction =
  OrderTx of Order
  | SplitAdjustmentTx of SplitAdjustment
  | CashDividendPaymentTx of CashDividendPayment

type TransactionLog = Vector<Transaction>


// trading strategy and state typeclasses
 
type BaseStrategyState = {
  previousTime: ZonedDateTime
  time: ZonedDateTime
  portfolio: Portfolio
  orders: Vector<Order>
  transactions: TransactionLog
  portfolioValueHistory: Vector<PortfolioValue>
}

// StrategyState typeclass
type StrategyState<'StateT> = {
  previousTime: 'StateT -> ZonedDateTime
  time: 'StateT -> ZonedDateTime
  portfolio: 'StateT -> Portfolio
  orders: 'StateT -> Vector<Order>
  transactions: 'StateT -> TransactionLog
  portfolioValueHistory: 'StateT -> Vector<PortfolioValue>   // todo, make this Vector<PortfolioValue> or LinkedList<PortfolioValue> ?

  // (time: ZonedDateTime) -> (principal: decimal) -> 'StateT
  initialize: ZonedDateTime -> decimal -> 'StateT

  withOrders: Vector<Order> -> 'StateT -> 'StateT
  withPortfolio: Portfolio -> 'StateT -> 'StateT
  withTransactions: TransactionLog -> 'StateT -> 'StateT
  withOrdersPortfolioTransactions: Vector<Order> -> Portfolio -> TransactionLog -> 'StateT -> 'StateT

  // (time: ZonedDateTime) -> (previousTime: ZonedDateTime) -> 'StateT -> 'StateT
  withTime: ZonedDateTime -> ZonedDateTime -> 'StateT -> 'StateT
  withPortfolioValueHistory: Vector<PortfolioValue> -> 'StateT -> 'StateT

  toString: 'StateT -> string
}

let toBaseStrategyState stateInterface state: BaseStrategyState =
  {
    previousTime = stateInterface.previousTime state
    time = stateInterface.time state
    portfolio = stateInterface.portfolio state
    orders = stateInterface.orders state
    transactions = stateInterface.transactions state
    portfolioValueHistory = stateInterface.portfolioValueHistory state
  }

let toBaseStrategyStates stateInterface states: seq<BaseStrategyState> =
  states |> Seq.map (toBaseStrategyState stateInterface)

// TradingStrategy typeclass
type TradingStrategy<'StrategyT, 'StateT> = {
  name: 'StrategyT -> string
  buildInitialState: 'StrategyT -> ('StrategyT -> Trial -> 'StateT)
  buildNextState: 'StrategyT -> ('StrategyT -> Trial -> 'StateT -> 'StateT)
  isFinalState: 'StrategyT -> ('StrategyT -> Trial -> 'StateT -> bool)
}


// trial result calculations

type TrialResult = {
  startTime: ZonedDateTime
  endTime: ZonedDateTime
  transactionLog: TransactionLog
  portfolioValueLog: PortfolioValues
  trialYield: Option<decimal>
  mfe: Option<decimal>
  mae: Option<decimal>
  dailyStdDev: Option<decimal>
}

let computeTrialYield (trial: Trial) (state: BaseStrategyState): Option<decimal> =
  state.portfolioValueHistory
  |> Vector.tryLast
  |> Option.map (fun pv -> pv.value / trial.principal)

let computeTrialMfe (trial: Trial) (state: BaseStrategyState): Option<decimal> =
  state.portfolioValueHistory
  |> Seq.reduceOption (maxBy (fun pv -> pv.value))
  |> Option.map (fun pv -> pv.value / trial.principal)

let computeTrialMae (trial: Trial) (state: BaseStrategyState): Option<decimal> =
  state.portfolioValueHistory
  |> Seq.reduceOption (minBy (fun pv -> pv.value))
  |> Option.map (fun pv -> pv.value / trial.principal)

let computeTrialStdDev (state: BaseStrategyState): Option<decimal> =
  let portfolioValueHistory = state.portfolioValueHistory
  if Seq.isEmpty portfolioValueHistory then
    None
  else
    Some <| Sample.stdDev (Seq.map (fun pv -> pv.value) portfolioValueHistory)
