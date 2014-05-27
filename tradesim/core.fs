module dke.tradesim.Core

open NodaTime

type SecurityId = int

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