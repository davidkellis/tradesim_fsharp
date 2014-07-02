module dke.tradesim.Portfolio

open NodaTime
open FSharpx

open Time
open Core
open Quotes
open CorporateActions

let stockValue (securityId: SecurityId) (qty: int64) (time: ZonedDateTime) (priorBarPriceFn: BarQuoteFn) (currentBarPriceFn: BarQuoteFn) dao: Option<decimal> =
  let bar = findEodBar time securityId dao
  Option.map 
    (fun (bar: Bar) ->
      let priceFn = if isInstantBetweenInclusive time bar.startTime bar.endTime then currentBarPriceFn else priorBarPriceFn
      let price = priceFn bar
      let priceObservationTime = bar.endTime
      let sharePrice = adjustPriceForCorporateActions price securityId priceObservationTime time dao
      decimal qty * sharePrice
    )
    bar

let portfolioValue (portfolio: Portfolio) (time: ZonedDateTime) (priorBarPriceFn: BarQuoteFn) (currentBarPriceFn: BarQuoteFn) dao: decimal = 
  let stockValues = Seq.map
                      (fun (KeyValue(symbol, qty)) -> stockValue symbol qty time priorBarPriceFn currentBarPriceFn dao |> Option.getOrElse 0M)
                      portfolio.stocks
  let totalStockValue = Seq.fold (+) 0M stockValues
  portfolio.cash + totalStockValue

