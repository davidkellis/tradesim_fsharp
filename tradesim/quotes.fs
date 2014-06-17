module dke.tradesim.Quotes

open C5
open NodaTime

open Stdlib
open Cache
open Core
open Time
open Database

let barSimQuoteCache = buildLruCache<string, decimal> 200
let getSimQuote = get barSimQuoteCache
let putSimQuote = put barSimQuoteCache

let barSimQuote (bar: Bar): decimal =
  let barId = sprintf "%i%s" bar.securityId <| dateTimeToTimestampStr bar.startTime
  let cachedQuote = getSimQuote barId
  match cachedQuote with
  | Some value -> value
  | None ->
      let newQuote = (bar.l + bar.h) / 2M
      putSimQuote barId newQuote
      newQuote

(*
 * Returns the most recent EOD bar for <symbol> as of <date-time>.
 * If the given <date-time> falls within the interval of a particular bar, then that bar is returned;
 * If the given <date-time> does not fall within the interval of a particular bar, then the most recent bar as of that time is returned.
 * The bar returned is not adjusted for splits or dividend payments.
 *
 * Assumes that there is a mongodb collection named "eods" containing the fields:
 *   s (ticker symbol),
 *   ts (timestamp representing the start of the interval that the bar represents)
 *   te (timestamp representing the end of the interval that the bar represents)
 * and that there is an ascending index of the form:
 *   index([
 *     [:s, 1],
 *     [:ts, 1]
 *   ],
 *   unique: true)
 *)
let queryEodBar (time: ZonedDateTime) (securityId: SecurityId) (adapter: DatabaseAdapter<_>) =
  Logging.info <| sprintf "queryEodBar %s %i" (time.ToString()) securityId
  adapter.queryEodBar time securityId

(*
 * Returns the most recent EOD bar for <symbol> occurring entirely before <date-time>.
 * Like query-eod-bar, except that it returns the most recent EOD bar that ended before the given <date-time>.
 *
 * Assumes that there is a mongodb collection named "eods" containing the fields:
 *   s (ticker symbol),
 *   ts (timestamp representing the start of the interval that the bar represents)
 *   te (timestamp representing the end of the interval that the bar represents)
 * and that there is an ascending index of the form:
 *   index([
 *     [:s, 1],
 *     [:te, 1]
 *   ],
 *   unique: true)
 *)
let queryEodBarPriorTo (time: ZonedDateTime) (securityId: SecurityId) (adapter: DatabaseAdapter<'connectionT>) =
  Logging.info <| sprintf "queryEodBarPriorTo %s %i" (time.ToString()) securityId
  adapter.queryEodBarPriorTo time securityId

let queryEodBars (securityId: SecurityId) (adapter: DatabaseAdapter<'connectionT>) =
  Logging.info <| sprintf "queryEodBars %i" securityId
  adapter.queryEodBars securityId

let queryEodBarsBetween (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime) (adapter: DatabaseAdapter<'connectionT>) connection: seq<Bar> =
  Logging.info <| sprintf "queryEodBarsBetween %i %s %s" securityId (earliestTime.ToString()) (latestTime.ToString())
  let t1 = currentTime None
  let result = adapter.queryEodBarsBetween securityId earliestTime latestTime connection
  let t2 = currentTime None
  Logging.verbose <| sprintf "Time: %s" (prettyFormatPeriod <| periodBetween t1 t2)
  result


type PriceHistory = TreeDictionary<int64, Bar>   // a price history is a collection of (timestamp -> Bar) pairs

let loadPriceHistoryFromBars (bars: seq<Bar>): PriceHistory =
  let priceHistory: PriceHistory = new TreeDictionary<int64, Bar>()
  Seq.iter (fun (bar: Bar) -> priceHistory.Add(dateTimeToTimestamp bar.startTime, bar)) bars
  priceHistory

let loadPriceHistory (securityId: SecurityId) (adapter: DatabaseAdapter<'connectionT>) = 
  composelr3 queryEodBars loadPriceHistoryFromBars

let loadPriceHistoryBetween (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime) (adapter: DatabaseAdapter<'connectionT>) = 
  composelr5 queryEodBarsBetween loadPriceHistoryFromBars

// todo, resume work here

//let mostRecentBar(priceHistory: PriceHistory, timestamp: Long): Option<Bar> = {
//  let mapEntry = priceHistory.floorEntry(timestamp)
//  Option(mapEntry).map(_.getValue)
//}
//
//let mostRecentBarFromYear(time: DateTime, securityId: SecurityId, year: Int): Option<Bar> = {
//  let priceHistory = findPriceHistory(year, securityId)
//  mostRecentBar(priceHistory, timestamp(time))
//}
//
//let findEodBar(time: DateTime, securityId: SecurityId): Option<Bar> = {
//  let year = time.getYear
//  let bar: Option<Bar> = mostRecentBarFromYear(time, securityId, year).orElse(mostRecentBarFromYear(time, securityId, year - 1))
//  bar.orElse(queryEodBar(time, securityId))
//}
//
//let findEodBarPriorTo(time: DateTime, securityId: SecurityId): Option<Bar> = {
//  let eodBar = findEodBar(time, securityId)
//  eodBar.flatMap { bar =>
//    if (isInstantBetweenInclusive(time, bar.startTime, bar.endTime))
//      findEodBar(bar.startTime.minus(millis(1)), securityId)
//    else Option(bar)
//  }
//}
//
//let findOldestEodBar(securityId: SecurityId)(implicit adapter: Adapter): Option<Bar> = {
//  info(s"findOldestEodBar($securityId)")
//  adapter.findOldestEodBar(securityId)
//}
//
//let findMostRecentEodBar(securityId: SecurityId)(implicit adapter: Adapter): Option<Bar> = {
//  info(s"findMostRecentEodBar($securityId)")
//  adapter.findMostRecentEodBar(securityId)
//}
//
//
//let priceHistoryCache = cache.buildLruCache(32, "priceHistoryCache")
//
//// loads up 5 years of price history
//let findPriceHistory(year: Int, securityId: SecurityId): PriceHistory = {
//  let startYear = year - year % 5
//  let priceHistoryId = securityId.toString ++ ":" ++ startYear.toString
//  let cachedPriceHistory = Option(priceHistoryCache.get(priceHistoryId))
//  cachedPriceHistory match {
//    case Some(priceHistoryElement) => priceHistoryElement.getObjectValue.asInstanceOf<PriceHistory>
//    case None =>
//      let endYear = startYear + 4
//      let newPriceHistory = loadPriceHistory(securityId,
//                                             datetime(startYear, 1, 1),
//                                             datetime(endYear, 12, 31, 23, 59, 59))    // load 5 calendar years of price history into a NavigableMap
//      priceHistoryCache.put(new Element(priceHistoryId, newPriceHistory))
//      newPriceHistory
//  }
//}