module dke.tradesim.Ordering

open NodaTime
open FSharpx

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

let setOrderQty (order: Order) (newQty: int64): Order = 
  match order with
  | MarketBuy details -> MarketBuy { details with qty = newQty }
  | MarketSell details -> MarketSell { details with qty = newQty }
  | LimitBuy details -> LimitBuy { details with qty = newQty }
  | LimitSell details -> LimitSell { details with qty = newQty }

let setLimitPrice (order: Order) (newLimitPrice: decimal): Order = 
  match order with
  | LimitBuy details -> LimitBuy { details with limitPrice = newLimitPrice }
  | LimitSell details -> LimitSell { details with limitPrice = newLimitPrice }
  | _ -> order

let sharesOnHand (portfolio: Portfolio) (securityId: SecurityId): int64 = portfolio.stocks.TryFind(securityId) |> Option.getOrElse 0L

let addCash (portfolio: Portfolio) (amount: decimal): Portfolio = { portfolio with cash = portfolio.cash + amount }

let setSharesOnHand (portfolio: Portfolio) (securityId: SecurityId) (qty: int64): Portfolio =
  { portfolio with stocks = portfolio.stocks.Add(securityId, qty) }

let cashOnHand (state: 'StateT) (stateInterface: StrategyState<'StateT>): decimal = (stateInterface.portfolio state).cash

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
    { portfolio with stocks = portfolio.stocks.Add(securityId, sharesHeld + orderQty)
                     cash = cashOnHand - purchaseCost commissionPerTrade commissionPerShare orderQty fillPrice }
  | MarketSell _ | LimitSell _ ->  // adjust the portfolio for a sale
    { portfolio with stocks = portfolio.stocks.Add(securityId, sharesHeld - orderQty)
                     cash = cashOnHand + saleProceeds commissionPerTrade commissionPerShare orderQty fillPrice }

let maxSharesPurchasable (trial: Trial)
                         (principal: decimal)
                         (time: ZonedDateTime)
                         (securityId: SecurityId)
                         (bestOfferPriceFn: PriceQuoteFn): Option<decimal> =
  let commissionPerTrade = trial.commissionPerTrade
  let commissionPerShare = trial.commissionPerShare
  bestOfferPriceFn time securityId |> Option.map (fun price -> integralQuotient (principal - commissionPerTrade) (price + commissionPerShare))

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

let cancelAllPendingOrders (currentState: 'stateT) (stateInterface: StrategyState<'stateT>): 'stateT = stateInterface.withOrders [| |]


// todo, resume work here. need an immutable array/vector that supports O(log n) insert/append
let buy (currentState: 'stateT) (time: ZonedDateTime) (securityId: SecurityId) (qty: int64) (stateInterface: StrategyState<'stateT>): 'stateT =
  let newOrders = (stateInterface.orders currentState). + MarketBuy {time = time; securityId = securityId; qty = qty; fillPrice = None}
  currentState.copy(orders = newOrders)

let buyImmediately<StateT <: State<StateT>>(currentState: StateT, securityId: SecurityId, qty: int64): StateT = buy(currentState, currentState.time, securityId, qty)

let buyEqually<StateT <: State<StateT>>(trial: Trial, currentState: StateT, securityIds: IndexedSeq<SecurityId>, bestOfferPriceFn: PriceQuoteFn): StateT = {
  let count = securityIds.length
  let cash = cashOnHand(currentState)
  let principalPerSecurity = cash / count
  securityIds.foldLeft(currentState) { (state, securityId) =>
    let qty = maxSharesPurchasable(trial, principalPerSecurity, currentState.time, securityId, bestOfferPriceFn)
    qty.map(qty => buyImmediately(state, securityId, floor(qty).toint64)).getOrElse(state)
  }
}

let limitBuy<StateT <: State<StateT>>(currentState: StateT, time: DateTime, securityId: SecurityId, qty: int64, limitPrice: decimal): StateT = {
  let newOrders = currentState.orders :+ LimitBuy(time, securityId, qty, limitPrice, None)
  currentState.copy(orders = newOrders)
}

// this should be merged with sellImmediately
let sell<StateT <: State<StateT>>(currentState: StateT, time: DateTime, securityId: SecurityId, qty: int64): StateT = {
  let newOrders = currentState.orders :+ MarketSell(time, securityId, qty, None)
  currentState.copy(orders = newOrders)
}

let sellImmediately<StateT <: State<StateT>>(currentState: StateT, securityId: SecurityId, qty: int64): StateT = sell(currentState, currentState.time, securityId, qty)

let limitSell<StateT <: State<StateT>>(currentState: StateT, time: DateTime, securityId: SecurityId, qty: int64, limitPrice: decimal): StateT = {
  let newOrders = currentState.orders :+ LimitSell(time, securityId, qty, limitPrice, None)
  currentState.copy(orders = newOrders)
}

let closeOpenStockPosition<StateT <: State<StateT>>(currentState: StateT, securityId: SecurityId): StateT = {
  let qtyOnHand = sharesOnHand(currentState.portfolio, securityId)
  qtyOnHand match {
    case qty if qty > 0 => sellImmediately(currentState, securityId, qtyOnHand)    // we own shares, so sell them
    case qty if qty < 0 => buyImmediately(currentState, securityId, -qtyOnHand)    // we owe a share debt, so buy those shares back (we negate qtyOnHand because it is negative, and we want to buy a positive quantity)
    case 0 => currentState
  }
}

let closeAllOpenStockPositions<StateT <: State<StateT>>(currentState: StateT): StateT = {
  let stocks = currentState.portfolio.stocks
  if (!stocks.isEmpty)
    stocks.keys.foldLeft(currentState)(closeOpenStockPosition)
  else
    currentState
}
