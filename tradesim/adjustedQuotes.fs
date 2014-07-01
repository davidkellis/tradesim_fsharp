module dke.tradesim.AdjustedQuotes

open NodaTime

open Core
open Quotes
open CorporateActions

let adjEodQuote (time: ZonedDateTime) (securityId: SecurityId) (priceFn: Bar -> decimal) dao: Option<decimal> =
  let bar = findEodBar time securityId dao
  Option.map 
    (fun bar ->
      let price = priceFn bar
      let priceObservationTime = bar.endTime
      adjustPriceForCorporateActions price securityId priceObservationTime time dao
    )
    bar

let adjEodClose (time: ZonedDateTime) (securityId: SecurityId) dao: Option<decimal> = adjEodQuote time securityId (fun bar -> bar.c) dao

let adjEodOpen (time: ZonedDateTime) (securityId: SecurityId) dao: Option<decimal> = adjEodQuote time securityId (fun bar -> bar.o) dao

let adjEodSimQuote (time: ZonedDateTime) (securityId: SecurityId) dao: Option<decimal> = adjEodQuote time securityId barSimQuote dao


let adjEodQuotePriorTo (time: ZonedDateTime) (securityId: SecurityId) (priceFn: Bar -> decimal) dao: Option<decimal> =
  let bar = findEodBarPriorTo time securityId dao
  Option.map
    (fun bar ->
      let price = priceFn bar
      let priceObservationTime = bar.endTime
      adjustPriceForCorporateActions price securityId priceObservationTime time dao
    )
    bar

let adjEodClosePriorTo (time: ZonedDateTime) (securityId: SecurityId) dao: Option<decimal> = adjEodQuotePriorTo time securityId (fun bar -> bar.c) dao

let adjEodOpenPriorTo (time: ZonedDateTime) (securityId: SecurityId) dao: Option<decimal> = adjEodQuotePriorTo time securityId (fun bar -> bar.o) dao

let adjEodSimQuotePriorTo (time: ZonedDateTime) (securityId: SecurityId) dao: Option<decimal> = adjEodQuotePriorTo time securityId barSimQuote dao
