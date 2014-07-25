module dke.tradesim.AdjustedQuotes

open NodaTime

open Core
open Quotes
open CorporateActions

let adjEodQuote dao (time: ZonedDateTime) (securityId: SecurityId) (priceFn: BarQuoteFn): Option<decimal> =
  let bar = findEodBar dao time securityId
  Option.map 
    (fun bar ->
      let price = priceFn bar
      let priceObservationTime = bar.endTime
      adjustPriceForCorporateActions dao price securityId priceObservationTime time
    )
    bar

let adjEodClose dao (time: ZonedDateTime) (securityId: SecurityId): Option<decimal> = adjEodQuote dao time securityId (fun bar -> bar.c)

let adjEodOpen dao (time: ZonedDateTime) (securityId: SecurityId): Option<decimal> = adjEodQuote dao time securityId (fun bar -> bar.o)

let adjEodSimQuote dao (time: ZonedDateTime) (securityId: SecurityId): Option<decimal> = adjEodQuote dao time securityId barSimQuote


let adjEodQuotePriorTo dao (time: ZonedDateTime) (securityId: SecurityId) (priceFn: BarQuoteFn): Option<decimal> =
  let bar = findEodBarPriorTo dao time securityId
  Option.map
    (fun bar ->
      let price = priceFn bar
      let priceObservationTime = bar.endTime
      adjustPriceForCorporateActions dao price securityId priceObservationTime time
    )
    bar

let adjEodClosePriorTo dao (time: ZonedDateTime) (securityId: SecurityId): Option<decimal> = adjEodQuotePriorTo dao time securityId (fun bar -> bar.c)

let adjEodOpenPriorTo dao (time: ZonedDateTime) (securityId: SecurityId): Option<decimal> = adjEodQuotePriorTo dao time securityId (fun bar -> bar.o)

let adjEodSimQuotePriorTo dao (time: ZonedDateTime) (securityId: SecurityId): Option<decimal> = adjEodQuotePriorTo dao time securityId barSimQuote
