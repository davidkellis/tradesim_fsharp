﻿module dke.tradesim.Ordering

open NodaTime
open FSharpx
open FSharpx.Collections

open Math
open Core

let orderSecurityId = function
  | MarketBuy o | MarketSell o -> o.securityId
  | LimitBuy o | LimitSell o -> o.securityId

let orderQty = function
  | MarketBuy o | MarketSell o -> o.qty
  | LimitBuy o | LimitSell o -> o.qty

let orderFillPrice = function
  | MarketBuy o | MarketSell o -> o.fillPrice
  | LimitBuy o | LimitSell o -> o.fillPrice

let isLimitOrder = function
  | LimitBuy _ | LimitSell _ -> true
  | _ -> false

let setOrderQty (newQty: int64) (order: Order): Order = 
  match order with
  | MarketBuy details -> MarketBuy { details with qty = newQty }
  | MarketSell details -> MarketSell { details with qty = newQty }
  | LimitBuy details -> LimitBuy { details with qty = newQty }
  | LimitSell details -> LimitSell { details with qty = newQty }

let setOrderFillPrice (newFillPrice: Option<decimal>) (order: Order): Order = 
  match order with
  | MarketBuy details -> MarketBuy { details with fillPrice = newFillPrice }
  | MarketSell details -> MarketSell { details with fillPrice = newFillPrice }
  | LimitBuy details -> LimitBuy { details with fillPrice = newFillPrice }
  | LimitSell details -> LimitSell { details with fillPrice = newFillPrice }

let setLimitPrice (newLimitPrice: decimal) (order: Order): Order = 
  match order with
  | LimitBuy details -> LimitBuy { details with limitPrice = newLimitPrice }
  | LimitSell details -> LimitSell { details with limitPrice = newLimitPrice }
  | _ -> order

let sharesOnHand (portfolio: Portfolio) (securityId: SecurityId): int64 = portfolio.stocks.TryFind(securityId) |> Option.getOrElse 0L

let addCash (amount: decimal) (portfolio: Portfolio): Portfolio = { portfolio with cash = portfolio.cash + amount }

let setSharesOnHand (securityId: SecurityId) (qty: int64) (portfolio: Portfolio) : Portfolio =
  { portfolio with stocks = portfolio.stocks.SetItem(securityId, qty) }

let cashOnHand (stateInterface: StrategyState<'StateT>) (state: 'StateT): decimal = (stateInterface.portfolio state).cash

let purchaseCost (commissionPerTrade: decimal) (commissionPerShare: decimal) (qty: int64) (price: decimal): decimal = (decimal qty * (price + commissionPerShare)) + commissionPerTrade

let purchaseCostAtTime (time: ZonedDateTime)
                       (securityId: SecurityId)
                       (qty: int64)
                       (commissionPerTrade: decimal)
                       (commissionPerShare: decimal)
                       (priceFn: PriceQuoteFn): Option<decimal> =
  priceFn time securityId |> Option.map (purchaseCost commissionPerTrade commissionPerShare qty)

let saleProceeds (commissionPerTrade: decimal) (commissionPerShare: decimal) (qty: int64) (price: decimal): decimal = (decimal qty * (price - commissionPerShare)) - commissionPerTrade

let saleProceedsAtTime (time: ZonedDateTime)
                       (securityId: SecurityId)
                       (qty: int64)
                       (commissionPerTrade: decimal)
                       (commissionPerShare: decimal)
                       (priceFn: PriceQuoteFn): Option<decimal> =
  priceFn time securityId |> Option.map (saleProceeds commissionPerTrade commissionPerShare qty)

let adjustPortfolioFromFilledOrder (trial: Trial) (portfolio: Portfolio) (order: Order): Portfolio =
  let commissionPerTrade = trial.commissionPerTrade
  let commissionPerShare = trial.commissionPerShare
  let securityId = orderSecurityId order
  let orderQty = orderQty order
  let fillPrice = orderFillPrice order |> Option.get
  let cashOnHand = portfolio.cash
  let sharesHeld = sharesOnHand portfolio securityId
  match order with
  | MarketBuy _ | LimitBuy _ ->   // adjust the portfolio for a purchase
    { portfolio with stocks = portfolio.stocks.SetItem(securityId, sharesHeld + orderQty)
                     cash = cashOnHand - purchaseCost commissionPerTrade commissionPerShare orderQty fillPrice }
  | MarketSell _ | LimitSell _ ->  // adjust the portfolio for a sale
    { portfolio with stocks = portfolio.stocks.SetItem(securityId, sharesHeld - orderQty)
                     cash = cashOnHand + saleProceeds commissionPerTrade commissionPerShare orderQty fillPrice }

let maxSharesPurchasable (trial: Trial)
                         (principal: decimal)
                         (time: ZonedDateTime)
                         (securityId: SecurityId)
                         (bestOfferPriceFn: PriceQuoteFn): Option<decimal> =
  let commissionPerTrade = trial.commissionPerTrade
  let commissionPerShare = trial.commissionPerShare
  bestOfferPriceFn time securityId |> Option.map (fun price -> Decimal.integralQuotient (principal - commissionPerTrade) (price + commissionPerShare))

let maxSharesPurchasableByPortfolio trial portfolio time securityId bestOfferPriceFn: Option<decimal> = maxSharesPurchasable trial portfolio.cash time securityId bestOfferPriceFn

let isMarketBuyOrderFillable (order: MarketOrderDetails) (time: ZonedDateTime) (trial: Trial) (portfolio: Portfolio) (purchaseFillPriceFn: PriceQuoteFn): bool =
  let cost = purchaseCostAtTime time order.securityId order.qty trial.commissionPerTrade trial.commissionPerShare purchaseFillPriceFn

  // todo: I think this requirement should be removed because http://www.21stcenturyinvestoreducation.com/page/tce/courses/course-101/005/001-cash-vs-margin.html
  //       says even cash accounts can temporarily have a negative cash balance as long as the necessary funds are depositied within 3 business days after
  //       the purchase.
  cost |> Option.map (fun cost -> cost <= portfolio.cash) |> Option.getOrElse false    // this condition is only applicable to cash-only accounts; this is allowed in margin accounts
  // the condition should be that the post-purchase cash balance should be within reasonable margin requirements.

let isMarketSellOrderFillable (order: MarketOrderDetails) (time: ZonedDateTime) (trial: Trial) (portfolio: Portfolio) (saleFillPriceFn: PriceQuoteFn): bool = 
  let proceeds = saleProceedsAtTime time order.securityId order.qty trial.commissionPerTrade trial.commissionPerShare saleFillPriceFn
  proceeds |> Option.map (fun proceeds -> proceeds >= 0.0M) |> Option.getOrElse false

let isLimitBuyOrderFillable (order: LimitOrderDetails) (time: ZonedDateTime) (trial: Trial) (portfolio: Portfolio) (purchaseFillPriceFn: PriceQuoteFn): bool = 
  let fillPrice = purchaseFillPriceFn time order.securityId
  fillPrice |> Option.map (fun fillPrice -> fillPrice <= order.limitPrice) |> Option.getOrElse false && isMarketBuyOrderFillable (limitToMarketOrderDetails order) time trial portfolio purchaseFillPriceFn

let isLimitSellOrderFillable (order: LimitOrderDetails) (time: ZonedDateTime) (trial: Trial) (portfolio: Portfolio) (saleFillPriceFn: PriceQuoteFn): bool =
  let fillPrice = saleFillPriceFn time order.securityId
  fillPrice |> Option.map (fun fillPrice -> fillPrice >= order.limitPrice) |> Option.getOrElse false && isMarketSellOrderFillable (limitToMarketOrderDetails order) time trial portfolio saleFillPriceFn

let isOrderFillable order time trial portfolio purchaseFillPriceFn saleFillPriceFn: bool =
  match order with
  | MarketBuy orderDetails -> isMarketBuyOrderFillable orderDetails time trial portfolio purchaseFillPriceFn
  | MarketSell orderDetails -> isMarketSellOrderFillable orderDetails time trial portfolio saleFillPriceFn
  | LimitBuy orderDetails -> isLimitBuyOrderFillable orderDetails time trial portfolio purchaseFillPriceFn
  | LimitSell orderDetails -> isLimitSellOrderFillable orderDetails time trial portfolio saleFillPriceFn

let computeOrderFillPrice (order: Order) (time: ZonedDateTime) (purchaseFillPriceFn: PriceQuoteFn) (saleFillPriceFn: PriceQuoteFn): Option<decimal> = 
  match order with
  | MarketBuy {securityId = sId} | LimitBuy {securityId = sId} -> purchaseFillPriceFn time sId
  | MarketSell {securityId = sId} | LimitSell {securityId = sId} -> saleFillPriceFn time sId

let cancelAllPendingOrders (stateInterface: StrategyState<'StateT>) (currentState: 'StateT): 'StateT = 
  currentState |> stateInterface.withOrders (Vector.empty)


let buy (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) (time: ZonedDateTime) (securityId: SecurityId) (qty: int64): 'StateT =
  let newOrder = MarketBuy {time = time; securityId = securityId; qty = qty; fillPrice = None}
  let currentOrders = stateInterface.orders currentState
  let newOrders = Vector.conj newOrder currentOrders
  currentState |> stateInterface.withOrders newOrders

let buyImmediately (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) (securityId: SecurityId) (qty: int64): 'StateT =
  let time = stateInterface.time currentState
  buy stateInterface currentState time securityId qty

let buyEqually (trial: Trial) 
               (stateInterface: StrategyState<'StateT>)
               (currentState: 'StateT) 
               (securityIds: Vector<SecurityId>) 
               (bestOfferPriceFn: PriceQuoteFn) 
               : 'StateT =
  let count = securityIds.Length
  let cash = cashOnHand stateInterface currentState
  let principalPerSecurity = cash / decimal count
  Vector.fold 
    (fun state securityId ->
      let qty = maxSharesPurchasable trial principalPerSecurity (stateInterface.time currentState) securityId bestOfferPriceFn
      qty |> Option.map (fun qty -> buyImmediately stateInterface state securityId (Decimal.floor qty |> int64)) |> Option.getOrElse(state)
    )
    currentState
    securityIds

let limitBuy (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) (time: ZonedDateTime) (securityId: SecurityId) (qty: int64) (limitPrice: decimal): 'StateT =
  let newOrder = LimitBuy {time = time; securityId = securityId; qty = qty; limitPrice = limitPrice; fillPrice = None}
  let currentOrders = stateInterface.orders currentState
  let newOrders = Vector.conj newOrder currentOrders
  currentState |> stateInterface.withOrders newOrders

// this should be merged with sellImmediately
let sell (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) (time: ZonedDateTime) (securityId: SecurityId) (qty: int64): 'StateT =
  let newOrder = MarketSell {time = time; securityId = securityId; qty = qty; fillPrice = None}
  let currentOrders = stateInterface.orders currentState
  let newOrders = Vector.conj newOrder currentOrders
  currentState |> stateInterface.withOrders newOrders

let sellImmediately (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) (securityId: SecurityId) (qty: int64): 'StateT =
  let time = stateInterface.time currentState
  sell stateInterface currentState time securityId qty

let limitSell (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) (time: ZonedDateTime) (securityId: SecurityId) (qty: int64) (limitPrice: decimal): 'StateT =
  let newOrder = LimitSell {time = time; securityId = securityId; qty = qty; limitPrice = limitPrice; fillPrice = None}
  let currentOrders = stateInterface.orders currentState
  let newOrders = Vector.conj newOrder currentOrders
  currentState |> stateInterface.withOrders newOrders

let closeOpenStockPosition (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) (securityId: SecurityId): 'StateT = 
  let portfolio = stateInterface.portfolio currentState
  let qtyOnHand = sharesOnHand portfolio securityId
  if qtyOnHand > 0L then
    sellImmediately stateInterface currentState securityId qtyOnHand    // we own shares, so sell them
  elif qtyOnHand < 0L then
    buyImmediately stateInterface currentState securityId -qtyOnHand    // we owe a share debt, so buy those shares back (we negate qtyOnHand because it is negative, and we want to buy a positive quantity)
  else
    currentState

let closeAllOpenStockPositions (stateInterface: StrategyState<'StateT>) (currentState: 'StateT): 'StateT =
  let stocks = (stateInterface.portfolio currentState).stocks
  if not stocks.IsEmpty then
    Seq.fold (fun state securityId -> closeOpenStockPosition stateInterface state securityId) currentState stocks.Keys
  else
    currentState
