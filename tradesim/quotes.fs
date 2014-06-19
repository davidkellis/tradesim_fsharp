module dke.tradesim.Quotes

open C5
open NodaTime
open FSharpx

open Stdlib
open Option
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

// note: compare this definition to the definition of loadPriceHistoryBetween. Which is preferable?
let loadPriceHistory<'connectionT> : SecurityId -> DatabaseAdapter<'connectionT> -> 'connectionT -> PriceHistory = 
  composelr3 queryEodBars loadPriceHistoryFromBars

// note: compare this definition to the definition of loadPriceHistory. Which is preferable?
let loadPriceHistoryBetween (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime) dbAdapter connection = 
  queryEodBarsBetween securityId earliestTime latestTime dbAdapter connection |> loadPriceHistoryFromBars


let mostRecentBar (priceHistory: PriceHistory) (timestamp: int64): Option<Bar> =
  priceHistory.TryWeakPredecessor(timestamp) |> outParamToOpt |> Option.map (fun kvPair -> kvPair.Value)

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
let priceHistoryCache = buildLruCache<string, PriceHistory> 32
let getPriceHistory = get priceHistoryCache
let putPriceHistory = put priceHistoryCache

// loads up 5 years of price history
let findPriceHistory (year: int) (securityId: SecurityId) dbAdapter connection: PriceHistory =
  let startYear = year - year % 5
  let priceHistoryId = sprintf "%i:%i" securityId startYear
  let cachedPriceHistory = getPriceHistory priceHistoryId
  match cachedPriceHistory with
  | Some priceHistory -> priceHistory
  | None ->
    let endYear = startYear + 4
    // load 5 calendar years of price history into a TreeDictionary
    let newPriceHistory = loadPriceHistoryBetween securityId <| datetime startYear 1 1 0 0 0 <| datetime endYear 12 31 23 59 59 <| dbAdapter <| connection
    putPriceHistory priceHistoryId newPriceHistory
    newPriceHistory

let mostRecentBarFromYear (time: ZonedDateTime) (securityId: SecurityId) (year: int) dbAdapter connection =
  let priceHistory = findPriceHistory year securityId dbAdapter connection
  mostRecentBar priceHistory <| dateTimeToTimestamp time

let findEodBar (time: ZonedDateTime) (securityId: SecurityId) dbAdapter connection: Option<Bar> =
  let year = time.Year
  mostRecentBarFromYear time securityId year dbAdapter connection 
  |> Option.orElseLazy (lazy (mostRecentBarFromYear time securityId (year - 1) dbAdapter connection))
  |> Option.orElseLazy (lazy (queryEodBar time securityId dbAdapter connection))

let findEodBarPriorTo (time: ZonedDateTime) (securityId: SecurityId) dbAdapter connection: Option<Bar> =
  findEodBar time securityId dbAdapter connection
  |> Option.flatMap 
    (fun bar ->
      if isInstantBetween time bar.startTime bar.endTime then
        let minimallyEarlierTime = bar.startTime - Duration.FromMilliseconds(1L)
        findEodBar minimallyEarlierTime securityId dbAdapter connection
      else
        Some bar
    )
