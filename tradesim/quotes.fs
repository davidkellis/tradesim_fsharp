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
let queryEodBar (time: ZonedDateTime) (securityId: SecurityId) (dao: Dao<_>) =
  Logging.info <| sprintf "queryEodBar %s %i" (time.ToString()) securityId
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
let queryEodBarPriorTo (time: ZonedDateTime) (securityId: SecurityId) (dao: Dao<_>) =
  Logging.info <| sprintf "queryEodBarPriorTo %s %i" (time.ToString()) securityId
  dao.queryEodBarPriorTo time securityId

let queryEodBars (securityId: SecurityId) (dao: Dao<_>) =
  Logging.info <| sprintf "queryEodBars %i" securityId
  dao.queryEodBars securityId

let queryEodBarsBetween (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime) (dao: Dao<_>): seq<Bar> =
  Logging.info <| sprintf "queryEodBarsBetween %i %s %s" securityId (earliestTime.ToString()) (latestTime.ToString())
  let t1 = currentTime None
  let result = dao.queryEodBarsBetween securityId earliestTime latestTime
  let t2 = currentTime None
  Logging.verbose <| sprintf "Time: %s" (prettyFormatPeriod <| periodBetween t1 t2)
  result

let findOldestEodBar (securityId: SecurityId) (dao: Dao<_>): Option<Bar> =
  info <| sprintf "findOldestEodBar(%i)" securityId
  dao.findOldestEodBar securityId

let findMostRecentEodBar (securityId: SecurityId) (dao: Dao<_>): Option<Bar> =
  info <| sprintf "findMostRecentEodBar(%i)" securityId
  (dao: Dao<_>).findMostRecentEodBar securityId


type PriceHistory = TreeDictionary<int64, Bar>   // a price history is a collection of (timestamp -> Bar) pairs

let loadPriceHistoryFromBars (bars: seq<Bar>): PriceHistory =
  let priceHistory: PriceHistory = new TreeDictionary<int64, Bar>()
  Seq.iter (fun (bar: Bar) -> priceHistory.Add(dateTimeToTimestamp bar.startTime, bar)) bars
  priceHistory

// note: compare this definition to the definition of loadPriceHistoryBetween. Which is preferable?
let loadPriceHistory (securityId: SecurityId) (dao: Dao<_>): PriceHistory = queryEodBars securityId dao |> loadPriceHistoryFromBars

// note: compare this definition to the definition of loadPriceHistory. Which is preferable?
let loadPriceHistoryBetween (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime) (dao: Dao<_>): PriceHistory = 
  queryEodBarsBetween securityId earliestTime latestTime dao |> loadPriceHistoryFromBars


let mostRecentBar (priceHistory: PriceHistory) (timestamp: int64): Option<Bar> =
  priceHistory.TryWeakPredecessor(timestamp) |> outParamToOpt |> Option.map (fun kvPair -> kvPair.Value)


let priceHistoryCache = buildLruCache<string, PriceHistory> 32
let getPriceHistory = get priceHistoryCache
let putPriceHistory = put priceHistoryCache

// loads up 5 years of price history
let findPriceHistory (year: int) (securityId: SecurityId) (dao: Dao<_>): PriceHistory =
  let startYear = year - year % 5
  let priceHistoryId = sprintf "%i:%i" securityId startYear
  let cachedPriceHistory = getPriceHistory priceHistoryId
  match cachedPriceHistory with
  | Some priceHistory -> priceHistory
  | None ->
    let endYear = startYear + 4
    // load 5 calendar years of price history into a TreeDictionary
    let newPriceHistory = loadPriceHistoryBetween securityId <| datetime startYear 1 1 0 0 0 <| datetime endYear 12 31 23 59 59 <| dao
    putPriceHistory priceHistoryId newPriceHistory
    newPriceHistory

let mostRecentBarFromYear (time: ZonedDateTime) (securityId: SecurityId) (year: int) (dao: Dao<_>) =
  let priceHistory = findPriceHistory year securityId dao
  mostRecentBar priceHistory <| dateTimeToTimestamp time

let findEodBar (time: ZonedDateTime) (securityId: SecurityId) (dao: Dao<_>): Option<Bar> =
  let year = time.Year
  mostRecentBarFromYear time securityId year dao
  |> Option.orElseLazy (lazy (mostRecentBarFromYear time securityId (year - 1) dao))
  |> Option.orElseLazy (lazy (queryEodBar time securityId dao))

let findEodBarPriorTo (time: ZonedDateTime) (securityId: SecurityId) (dao: Dao<_>): Option<Bar> =
  findEodBar time securityId dao
  |> Option.flatMap 
    (fun bar ->
      if isInstantBetween time bar.startTime bar.endTime then
        let minimallyEarlierTime = bar.startTime - Duration.FromMilliseconds(1L)
        findEodBar minimallyEarlierTime securityId dao
      else
        Some bar
    )
