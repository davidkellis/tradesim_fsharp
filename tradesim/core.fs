module dke.tradesim.Core

open NodaTime

open Time

type SecurityId = int

type Industry = { name: string }
type Sector = { name: string }

type Exchange = {
  id: Option<int>
  label: string
  name: Option<string>
}

type Security = {
  id: Option<int>
  bbGid: string
  bbGcid: string
  kind: string
  symbol: string
  name: string
  startDate: Option<datestamp>
  endDate: Option<datestamp>
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