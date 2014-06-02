module dke.tradesim.Core

open NodaTime

type SecurityId = int

type Exchange = {
  id: Option<int>
  label: string
  name: Option<string>
}

type Bar = {
  securityId: SecurityId
  startTime: ZonedDateTime
  endTime: ZonedDateTime
  o: decimal
  h: decimal
  l: decimal
  c: decimal
  volume: int64
}