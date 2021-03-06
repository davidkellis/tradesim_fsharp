﻿module dke.tradesim.CorporateActions

open C5
open FSharpx.Collections
open FSharpx
open NodaTime

open Math
open Vector
open Cache
open Logging
open Time
open Core
open Ordering
open Database
open Quotes

type AdjustmentFactor = {corporateAction: CorporateAction; priorEodBar: Option<Bar>; adjustmentFactor: decimal}
type QtyAdjustmentFactor = {corporateAction: CorporateAction; adjustmentFactor: decimal}

let corporateActionSecurityId = function
  | SplitCA split -> split.securityId
  | CashDividendCA dividend -> dividend.securityId

let corporateActionExDate = function
  | SplitCA split -> split.exDate
  | CashDividendCA dividend -> dividend.exDate

let queryCorporateActions dao (securityIds: seq<SecurityId>): array<CorporateAction> =
  debugL <| lazy (sprintf "queryCorporateActions %s" (String.joinInts "," securityIds))
  dao.queryCorporateActions securityIds |> Seq.toArray

let queryCorporateActionsBetween dao (securityIds: seq<int>) (startTime: ZonedDateTime) (endTime: ZonedDateTime): array<CorporateAction> =
  debugL <| lazy (sprintf "queryCorporateActionsBetween %s %s %s" (String.joinInts "," securityIds) (dateTimeToTimestampStr startTime) (dateTimeToTimestampStr endTime))
  dao.queryCorporateActionsBetween securityIds startTime endTime |> Seq.toArray

type CorporateActionHistory = TreeDictionary<datestamp, CorporateAction>

let loadCorporateActionHistory dao (securityId: SecurityId): CorporateActionHistory =
  let corporateActions = queryCorporateActions dao [securityId]
  let corporateActionHistory = new CorporateActionHistory()
  Seq.iter 
    (fun ca -> corporateActionHistory.Add(localDateToDatestamp <| corporateActionExDate ca, ca) )
    corporateActions
  corporateActionHistory


let corporateActionCache = buildLruCache<SecurityId, CorporateActionHistory> 32
let getCorporateActionHistory = get corporateActionCache
let putCorporateActionHistory = put corporateActionCache


let findCorporateActionHistory dao (securityId: SecurityId): CorporateActionHistory =
  let cachedCorporateActionHistory = getCorporateActionHistory securityId
  match cachedCorporateActionHistory with
  | Some corporateActionHistory -> corporateActionHistory
  | None ->
      let newCorporateActionHistory = loadCorporateActionHistory dao securityId
      putCorporateActionHistory securityId newCorporateActionHistory
      newCorporateActionHistory

let findCorporateActionsFromHistory (history: CorporateActionHistory) (startTime: ZonedDateTime) (endTime: ZonedDateTime): Vector<CorporateAction> =
  let startTimestamp = dateTimeToDatestamp startTime
  let endTimestamp = (dateTimeToDatestamp endTime) + 1    // add 1 because RangeFromTo includes the start and excludes the end, so adding 1 ensures that we include the end
  let subHistory = history.RangeFromTo(startTimestamp, endTimestamp)
//  let corporateActions = Seq.mapIEnumerator (fun (pair: KeyValuePair<datestamp, CorporateAction>) -> pair.Value) (subHistory.GetEnumerator()) |> Seq.cache
  Vector.mapIEnumerator (fun (pair: KeyValuePair<datestamp, CorporateAction>) -> pair.Value) (subHistory.GetEnumerator())
//    println(s"findCorporateActionsFromHistory(history, $startTime, $endTime) -> ${corporateActions.toVector}")

let findCorporateActionsForSecurity dao (securityId: SecurityId) (startTime: ZonedDateTime) (endTime: ZonedDateTime): Vector<CorporateAction> =
  let history = findCorporateActionHistory dao securityId
  findCorporateActionsFromHistory history startTime endTime

let findCorporateActions dao (securityIds: seq<SecurityId>) (startTime: ZonedDateTime) (endTime: ZonedDateTime): Vector<CorporateAction> =
  Vector.flatMapSeq (fun securityId -> findCorporateActionsForSecurity dao securityId startTime endTime) securityIds

let findEodBarPriorToCorporateAction dao (corporateAction: CorporateAction): Option<Bar> =
  findEodBarPriorTo dao <| midnightOnDate (corporateActionExDate corporateAction) <| corporateActionSecurityId corporateAction

(*
 * See http://www.crsp.com/documentation/product/stkind/definitions/factor_to_adjust_price_in_period.html for implementation notes.
 * Returns an adjustment factor that:
 * 1. when multiplied by an unadjusted stock price, yields an adjusted stock price. i.e. unadjusted-price * adjustment-factor = adjusted-price
 * 2. when divided into an unadjusted share count, yields an adjusted share count. i.e. unadjusted-qty / adjustment-factor = adjusted-qty
 *)
let computeSplitAdjustmentFactor (splitRatio: decimal): decimal = 1M / splitRatio

(*
* priorAdjustmentFactors is a sequence of AdjustmentFactor(corporate-action, prior-eod-bar, adjustment-factor) tuples ordered in ascending
*   (i.e. oldest to most recent) order of the corporate action's ex-date.
* Assumes priorAdjustmentFactors is sorted in order of ascending order of ex-date
*)
let computeCumulativeDividendAdjustmentFactor (dividend: CashDividend) (priorEodBar: Bar) (priorAdjustmentFactors: Vector<AdjustmentFactor>): decimal =
  let adjustmentFactorsInDescendingOrderOfExDate = Vector.rev priorAdjustmentFactors
  let applicableAdjustmentFactors = Vector.takeWhile (fun adjFactor -> Option.get adjFactor.priorEodBar = priorEodBar) adjustmentFactorsInDescendingOrderOfExDate
  applicableAdjustmentFactors
  |> Vector.map (fun adjFactor -> adjFactor.adjustmentFactor)
  |> Vector.fold (*) 1M

(*
 * See http://www.crsp.com/documentation/product/stkind/definitions/factor_to_adjust_price_in_period.html for implementation notes.
 * <prior-eod-bar> is the most recent EOD bar prior to the ex-date of <dividend>
 *
 * Returns an adjustment factor that:
 * 1. when multiplied by an unadjusted stock price, yields an adjusted stock price. i.e. unadjusted-price * adjustment-factor = adjusted-price
 * 2. when divided into an unadjusted share count, yields an adjusted share count. i.e. unadjusted-qty / adjustment-factor = adjusted-qty
 *)
let computeDividendAdjustmentFactor (dividend: CashDividend) (priorEodBar: Option<Bar>) (priorAdjustmentFactors: Vector<AdjustmentFactor>): decimal =
  priorEodBar
  |> Option.map (fun eodBar -> 1M - dividend.amount / (eodBar.c * computeCumulativeDividendAdjustmentFactor dividend eodBar priorAdjustmentFactors) )
  |> Option.getOrElse 1M

let computeAdjustmentFactor (corporateAction: CorporateAction) (priorEodBar: Option<Bar>) (priorAdjustmentFactors: Vector<AdjustmentFactor>): decimal =
  match corporateAction with
  | SplitCA {ratio = ratio} -> computeSplitAdjustmentFactor ratio
  | CashDividendCA div -> computeDividendAdjustmentFactor div priorEodBar priorAdjustmentFactors


(*
 * Returns a sequence of <corporate-action, prior-eod-bar, adjustment-factor> tuples ordered in ascending (i.e. oldest to most recent) order of the corporate action's ex-date.
 * The first element of the tuple, <corporate-action> is the corporate action from which the <adjustment-factor> is computed.
 * The second element of the tuple, <prior-eod-bar> is the most recent EOD bar prior to the <corporate-action>.
 * The last element of the tuple, <adjustment-factor> is the adjustment factor for the given <corporate-action>.
 * NOTE:
 *   The <adjustment-factor> can be
 *   multiplied by a particular unadjusted historical price in order to compute a corporate-action-adjusted historical price. A given unadjusted
 *   historical share count can be divided by the <adjustment-factor> to compute the associated corporate-action-adjusted historical share count (e.g.
 *   to produce an adjusted share volume or an adjusted "shares outstanding" measurement).
 *   Each adjustment-factor is not cumulative, it is specifically tied to a particular corporate-action.
 *   The definition of the adjustment-factor is taken from http://www.crsp.com/documentation/product/stkind/definitions/factor_to_adjust_price_in_period.html:
 *   "Factor from a base date used to adjust prices after distributions so that equivalent comparisons can be made between prices before and after the distribution."
 *)
let priceAdjustmentFactors dao (securityId: SecurityId) (startTime: ZonedDateTime) (endTime: ZonedDateTime): Vector<AdjustmentFactor> =
  if startTime < endTime then
    let corporateActions = findCorporateActionsForSecurity dao securityId startTime endTime     // corporate actions ordered from oldest to newest
    let corporateActionEodBarPairs = Vector.map (fun corporateAction -> (corporateAction, findEodBarPriorToCorporateAction dao corporateAction) ) corporateActions
    Vector.fold
      (fun adjustmentFactors (corporateAction, priorEodBar) -> 
         let adjustmentFactor = {corporateAction = corporateAction; 
                                 priorEodBar = priorEodBar; 
                                 adjustmentFactor = computeAdjustmentFactor corporateAction priorEodBar adjustmentFactors}
         Vector.conj adjustmentFactor adjustmentFactors
      )
      Vector.empty<AdjustmentFactor>
      corporateActionEodBarPairs
  else
    Vector.empty<AdjustmentFactor>

// computes a cumulative price adjustment factor
let cumulativePriceAdjustmentFactor dao (securityId: SecurityId) (startTime: ZonedDateTime) (endTime: ZonedDateTime): decimal =
  priceAdjustmentFactors dao securityId startTime endTime
  |> Vector.map (fun adjFactor -> adjFactor.adjustmentFactor)
  |> Vector.fold (*) 1M

(*
 * Given a price, <price>, of <symbol> that was observed at <price-observation-time>,
 * returns an adjusted price that (using <price> as a base price) has been adjusted for corporate actions that take effect between
 * <price-observation-time> and <adjustment-time>.
 * Assumes <price-observation-time> occurred strictly before <adjustment-time> (i.e. <price-observation-time> is strictly
 * older than <adjustment-time>).
 * This function can be interpreted as:
 * "Adjust the price, <price>, of <symbol>, that was observed at <price-observation-time> for corporate actions that took effect between
 * <price-observation-time> and <adjustment-time>. The adjusted price is the price that one would expect for <symbol> to trade at as of
 * <adjustment-time>"
 * NOTE:
 *   See http://www.investopedia.com/ask/answers/06/adjustedclosingprice.asp#axzz24Wa9LgDj for instructions on how to adjust a price for splits.
 *   See http://www.quantshare.com/sa-112-stock-split-dividend
 *   or
 *   http://help.yahoo.com/kb/index?locale=en_US&page=content&y=PROD_FIN&id=SLN2311&impressions=true
 *   http://help.yahoo.com/kb/index?locale=en_US&page=content&y=PROD_FIN&id=SLN2311&actp=lorax&pir=wMsp3EFibUlGxLjgTY.StSPNcMXGv318Io7yMwp2vXYNYOLFM2Y-
 *   for instructions on how to adjust a price for cash dividends.
 *)
let adjustPriceForCorporateActions dao (price: decimal) (securityId: SecurityId) (priceObservationTime: ZonedDateTime) (adjustmentTime: ZonedDateTime): decimal =
  price * cumulativePriceAdjustmentFactor dao securityId priceObservationTime adjustmentTime

(*
 * Given a portfolio and split, this function applies the split to the portfolio and returns a split-adjusted portfolio.
 * Note:
 *   new holdings = old holdings * split ratio
 *)
let adjustPortfolioForSplit dao (split: Split) (stateInterface: StrategyState<'StateT>) (currentState: 'StateT): 'StateT =
  let portfolio = stateInterface.portfolio currentState
  let securityId = split.securityId
  let exDate = split.exDate
  let splitRatio = split.ratio
  let qty = sharesOnHand portfolio securityId
  let adjQty = decimal qty * splitRatio
  let adjSharesOnHandDecimal = floor adjQty
  let adjSharesOnHand = int64 adjSharesOnHandDecimal
  let fractionalShareQty = adjQty - adjSharesOnHandDecimal
  let eodBar = findEodBarPriorTo dao (midnightOnDate exDate) securityId
  eodBar
  |> Option.map
    (fun eodBar ->
      let closingPrice = eodBar.c
      let splitAdjustedSharePrice = adjustPriceForCorporateActions dao closingPrice securityId eodBar.endTime (midnightOnDate exDate)
      let fractionalShareCashValue = fractionalShareQty * splitAdjustedSharePrice
      let adjustedPortfolio = portfolio |> setSharesOnHand securityId adjSharesOnHand |> addCash fractionalShareCashValue
      let updatedTransactionLog = Vector.conj
                                    (SplitAdjustmentTx {securityId = split.securityId; exDate = split.exDate; ratio = split.ratio; adjustmentTime = (stateInterface.time currentState); shareQtyDelta = adjSharesOnHand - qty; cashPayout = fractionalShareCashValue})
                                    (stateInterface.transactions currentState)
      currentState |> stateInterface.withPortfolio adjustedPortfolio |> stateInterface.withTransactions updatedTransactionLog
    )
  |> Option.getOrElse currentState

// returns the amount of cash the given portfolio is entitled to receive from the given cash-dividend
let computeDividendPaymentAmount (cashDividend: CashDividend) (sharesOnHand: int64): decimal = decimal sharesOnHand * cashDividend.amount

let adjustPortfolioForCashDividend (dividend: CashDividend) (stateInterface: StrategyState<'StateT>) (currentState: 'StateT): 'StateT =
  let portfolio = stateInterface.portfolio currentState
  let qty = sharesOnHand portfolio dividend.securityId
  let dividendPaymentAmount = computeDividendPaymentAmount dividend qty
  let adjustedPortfolio = addCash dividendPaymentAmount portfolio
  let updatedTransactionLog = Vector.conj 
                                (CashDividendPaymentTx {securityId = dividend.securityId; exDate = dividend.exDate; payableDate = dividend.payableDate; amountPerShare = dividend.amount; adjustmentTime = (stateInterface.time currentState); shareQty = qty; total = dividendPaymentAmount})
                                (stateInterface.transactions currentState)
  currentState |> stateInterface.withPortfolio adjustedPortfolio |> stateInterface.withTransactions updatedTransactionLog

let adjustPortfolio dao (corporateAction: CorporateAction) (stateInterface: StrategyState<'StateT>) (currentState: 'StateT): 'StateT =
  match corporateAction with
  | SplitCA split -> adjustPortfolioForSplit dao split stateInterface currentState
  | CashDividendCA dividend -> adjustPortfolioForCashDividend dividend stateInterface currentState

let adjustPortfolioForCorporateActions dao (stateInterface: StrategyState<'StateT>) (currentState: 'StateT) (earlierObservationTime: ZonedDateTime) (laterObservationTime: ZonedDateTime): 'StateT =
  let portfolio = stateInterface.portfolio currentState
  let securityIds = portfolio.stocks.Keys
  let corporateActions = findCorporateActions dao securityIds earlierObservationTime laterObservationTime
//    println(s"********* Corporate Actions (for portfolio): $corporateActions for $symbols ; between $earlierObservationTime and $laterObservationTime")
  Vector.fold
    (fun updatedState corporateAction -> adjustPortfolio dao corporateAction stateInterface updatedState)
    currentState
    corporateActions


let adjustOpenOrderForSplit (split: Split) (openOrder: Order): Order =
  let splitRatio = split.ratio
  let qty = orderQty openOrder
  let adjQty = floor (decimal qty * splitRatio) |> int64
  match openOrder with
  | LimitBuy o | LimitSell o ->
    let limitPrice = o.limitPrice
    let adjLimitPrice = limitPrice / splitRatio
    openOrder |> setOrderQty adjQty |> setLimitPrice adjLimitPrice
  | _ -> setOrderQty adjQty openOrder

let adjustOpenOrderForCashDividend (dividend: CashDividend) (openOrder: Order): Order = openOrder

let adjustOpenOrder (corporateAction: CorporateAction) (openOrder: Order): Order =
  match corporateAction with
  | SplitCA split -> adjustOpenOrderForSplit split openOrder
  | CashDividendCA dividend -> adjustOpenOrderForCashDividend dividend openOrder

let adjustOpenOrdersForCorporateActions dao (openOrders: Vector<Order>) (earlierObservationTime: ZonedDateTime) (laterObservationTime: ZonedDateTime): Vector<Order> =
  if (Vector.isEmpty openOrders) then
    Vector.empty<Order>
  else
    let securityIds = Vector.map orderSecurityId openOrders
    let corporateActions = findCorporateActions dao securityIds earlierObservationTime laterObservationTime
    let corporateActionsPerSymbol = Vector.groupIntoMapBy corporateActionSecurityId corporateActions
//      println(s"********* Corporate Actions (for open orders): $corporateActions for $symbols ; between $earlierObservationTime and $laterObservationTime")
    Vector.map
      (fun openOrder ->
        let corporateActionsForSymbol = Map.tryFind (orderSecurityId openOrder) corporateActionsPerSymbol |> Option.getOrElse Vector.empty<CorporateAction>
        Vector.fold
          (fun order corporateAction -> adjustOpenOrder corporateAction order)
          openOrder
          corporateActionsForSymbol
      )
      openOrders


let computeShareQtyAdjustmentFactor(corporateAction: CorporateAction): decimal =
  match corporateAction with
  | SplitCA {ratio = ratio} -> computeSplitAdjustmentFactor ratio
  | CashDividendCA _ -> 1M

(*
 * Returns a sequence of AdjustmentFactors ordered in ascending (i.e. oldest to most recent) order of the corporate action's ex-date.
 * The first element of the tuple, <corporate-action> is the corporate action from which the <adjustment-factor> is computed.
 * The last element of the tuple, <adjustment-factor> is the adjustment factor for the given <corporate-action>.
 * NOTE:
 *   A given unadjusted historical share count can be divided by the <adjustment-factor> to compute the associated
 *   corporate-action-adjusted historical share count (e.g. to produce an adjusted share volume or an adjusted "shares outstanding"
 *   measurement).
 *   Each adjustment-factor is not cumulative, it is specifically tied to a particular corporate-action.
 *   The definition of the adjustment-factor is taken from http://www.crsp.com/documentation/product/stkind/definitions/factor_to_adjust_price_in_period.html:
 *   "Factor from a base date used to adjust prices after distributions so that equivalent comparisons can be made between prices
 *   before and after the distribution."
 *)
let shareQtyAdjustmentFactors dao (securityId: SecurityId) (earlierObservationTime: ZonedDateTime) (laterObservationTime: ZonedDateTime): Vector<QtyAdjustmentFactor> = 
  if earlierObservationTime < laterObservationTime then
    let corporateActions = findCorporateActionsForSecurity dao securityId earlierObservationTime laterObservationTime
    Vector.fold
      (fun qtyAdjustmentFactors corporateAction -> Vector.conj {corporateAction = corporateAction; adjustmentFactor = computeShareQtyAdjustmentFactor corporateAction} qtyAdjustmentFactors )
      Vector.empty<QtyAdjustmentFactor>
      corporateActions
  else
    Vector.empty<QtyAdjustmentFactor>

// computes a cumulative share quantity adjustment factor
let cumulativeShareQtyAdjustmentFactor dao (securityId: SecurityId) (earlierObservationTime: ZonedDateTime) (laterObservationTime: ZonedDateTime): decimal =
  shareQtyAdjustmentFactors dao securityId earlierObservationTime laterObservationTime
  |> Vector.map (fun qtyAdjFactor -> qtyAdjFactor.adjustmentFactor)
  |> Vector.fold (*) 1M

// returns a split adjusted share quantity, given an unadjusted share quantity
let adjustShareQtyForCorporateActions dao (unadjustedQty: decimal) (securityId: SecurityId) (earlierObservationTime: ZonedDateTime) (laterObservationTime: ZonedDateTime): decimal =
  unadjustedQty / cumulativeShareQtyAdjustmentFactor dao securityId earlierObservationTime laterObservationTime

