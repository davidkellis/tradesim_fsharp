module dke.tradesim.Core

open NodaTime

open Time

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
  fillPrice: Option<decimal>
  limitPrice: Option<decimal>
}

type Order =
  MarketBuy of MarketOrderDetails
  | MarketSell of MarketOrderDetails
  | LimitBuy of LimitOrderDetails
  | LimitSell of LimitOrderDetails

type StockHoldings = Map<SecurityId, int64>
type Portfolio = {
  cash: decimal
  stocks: StockHoldings
}

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

type TransactionLog = seq<Transaction>


// trading strategy and state typeclasses
 
// StrategyState typeclass
type StrategyState<'t> = {
  previousTime: 't -> ZonedDateTime
  time: 't -> ZonedDateTime
  portfolio: 't -> Portfolio
  orders: 't -> array<Order>
  transactions: 't -> TransactionLog
  portfolioValueHistory: 't -> seq<PortfolioValue>

  // (time: ZonedDateTime) -> (principal: decimal) -> 't
  initialize: ZonedDateTime -> decimal -> 't

  // (previousTime: ZonedDateTime) -> (time: ZonedDateTime) -> (portfolio: Portfolio) -> (orders: array<Order>) -> (transaction: TransactionLog) -> (portfolioValueHistory: seq<PortfolioValue>) -> 't
  copy: ZonedDateTime -> ZonedDateTime -> Portfolio -> array<Order> -> TransactionLog -> seq<PortfolioValue> -> 't
}

// TradingStrategy typeclass
type TradingStrategy<'strategyT, 'stateT> = {
  name: 'strategyT -> string
  buildInitialState: 'strategyT -> ('strategyT -> Trial -> 'stateT)
  buildNextState: 'strategyT -> ('strategyT -> Trial -> 'stateT -> 'stateT)
  isFinalState: 'strategyT -> ('strategyT -> Trial -> 'stateT -> bool)
}
