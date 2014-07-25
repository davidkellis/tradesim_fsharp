module dke.tradesim.Portfolio

open NodaTime
open FSharpx

open Time
open Core
open Quotes
open CorporateActions

let stockValue dao (securityId: SecurityId) (qty: int64) (time: ZonedDateTime) (priorBarPriceFn: BarQuoteFn) (currentBarPriceFn: BarQuoteFn): Option<decimal> =
  let bar = findEodBar dao time securityId
  Option.map 
    (fun (bar: Bar) ->
      let priceFn = if isInstantBetweenInclusive time bar.startTime bar.endTime then currentBarPriceFn else priorBarPriceFn
      let price = priceFn bar
      let priceObservationTime = bar.endTime
      let sharePrice = adjustPriceForCorporateActions dao price securityId priceObservationTime time
      decimal qty * sharePrice
    )
    bar

let portfolioValue dao (portfolio: Portfolio) (time: ZonedDateTime) (priorBarPriceFn: BarQuoteFn) (currentBarPriceFn: BarQuoteFn): decimal = 
  let stockValues = Seq.map
                      (fun (KeyValue(symbol, qty)) -> stockValue dao symbol qty time priorBarPriceFn currentBarPriceFn |> Option.getOrElse 0M)
                      portfolio.stocks
  let totalStockValue = Seq.fold (+) 0M stockValues
  portfolio.cash + totalStockValue
