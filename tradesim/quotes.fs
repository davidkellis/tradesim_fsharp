module dke.tradesim.Quotes

open C5
open NodaTime
open FSharpx

open Stdlib
open Option
open Cache
open Logging
open Core
open Time
open Database

type BarQuoteFn = Bar -> decimal

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
let queryEodBar dao (time: ZonedDateTime) (securityId: SecurityId) =
  Logging.debug <| sprintf "queryEodBar %s %i" (time.ToString()) securityId
  dao.queryEodBar time securityId

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
let queryEodBarPriorTo dao (time: ZonedDateTime) (securityId: SecurityId) =
  Logging.debug <| sprintf "queryEodBarPriorTo %s %i" (time.ToString()) securityId
  dao.queryEodBarPriorTo time securityId

let queryEodBars dao (securityId: SecurityId) =
  Logging.debug <| sprintf "queryEodBars %i" securityId
  dao.queryEodBars securityId

let queryEodBarsBetween dao (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime): seq<Bar> =
  Logging.debug <| sprintf "queryEodBarsBetween %i %s %s" securityId (earliestTime.ToString()) (latestTime.ToString())
//  let t1 = currentTime None
  let result = dao.queryEodBarsBetween securityId earliestTime latestTime
//  let t2 = currentTime None
//  Logging.verbose <| sprintf "Time: %s" (formatPeriod <| periodBetween t1 t2)
  result

let findOldestEodBar dao (securityId: SecurityId): Option<Bar> =
  debug <| sprintf "findOldestEodBar(%i)" securityId
  dao.findOldestEodBar securityId

let findMostRecentEodBar dao (securityId: SecurityId): Option<Bar> =
  debug <| sprintf "findMostRecentEodBar(%i)" securityId
  dao.findMostRecentEodBar securityId


type PriceHistory = TreeDictionary<timestamp, Bar>   // a price history is a collection of (timestamp -> Bar) pairs

let loadPriceHistoryFromBars (bars: seq<Bar>): PriceHistory =
  let priceHistory: PriceHistory = new PriceHistory()
  Seq.iter (fun (bar: Bar) -> priceHistory.Add(dateTimeToTimestamp bar.startTime, bar)) bars
  priceHistory

// note: compare this definition to the definition of loadPriceHistoryBetween. Which is preferable?
let loadPriceHistory dao (securityId: SecurityId): PriceHistory = queryEodBars dao securityId |> loadPriceHistoryFromBars

// note: compare this definition to the definition of loadPriceHistory. Which is preferable?
let loadPriceHistoryBetween dao (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime): PriceHistory = 
  queryEodBarsBetween dao securityId earliestTime latestTime |> loadPriceHistoryFromBars


let mostRecentBar (priceHistory: PriceHistory) (timestamp: int64): Option<Bar> =
  priceHistory.TryWeakPredecessor(timestamp) |> outParamToOpt |> Option.map (fun kvPair -> kvPair.Value)


let priceHistoryCache = buildLruCache<string, PriceHistory> 32
let getPriceHistory = get priceHistoryCache
let putPriceHistory = put priceHistoryCache

// loads up 5 years of price history
let findPriceHistory dao (year: int) (securityId: SecurityId): PriceHistory =
  let startYear = year - year % 5
  let priceHistoryId = sprintf "%i:%i" securityId startYear
  let cachedPriceHistory = getPriceHistory priceHistoryId
  match cachedPriceHistory with
  | Some priceHistory -> priceHistory
  | None ->
    let endYear = startYear + 4
    // load 5 calendar years of price history into a TreeDictionary
    let newPriceHistory = loadPriceHistoryBetween dao securityId <| datetime startYear 1 1 0 0 0 <| datetime endYear 12 31 23 59 59
    putPriceHistory priceHistoryId newPriceHistory
    newPriceHistory

let mostRecentBarFromYear dao (time: ZonedDateTime) (securityId: SecurityId) (year: int) =
  let priceHistory = findPriceHistory dao year securityId
  mostRecentBar priceHistory <| dateTimeToTimestamp time

let findEodBar dao (time: ZonedDateTime) (securityId: SecurityId): Option<Bar> =
  let year = time.Year
  mostRecentBarFromYear dao time securityId year
  |> Option.orElseLazy (lazy (mostRecentBarFromYear dao time securityId (year - 1)))
  |> Option.orElseLazy (lazy (queryEodBar dao time securityId))

let findEodBarPriorTo dao (time: ZonedDateTime) (securityId: SecurityId): Option<Bar> =
  findEodBar dao time securityId
  |> Option.flatMap 
    (fun bar ->
      if isInstantBetween time bar.startTime bar.endTime then
        let minimallyEarlierTime = bar.startTime - Duration.FromMilliseconds(1L)
        findEodBar dao minimallyEarlierTime securityId
      else
        Some bar
    )
