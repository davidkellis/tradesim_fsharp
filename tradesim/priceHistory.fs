module dke.tradesim.PriceHistory

open NodaTime
open FSharpx

open Core
open Time
open Quotes

// Returns the interval of time that spans the full price history of a particular symbol
let priceHistoryInterval dao (securityId: SecurityId): Option<Interval> =
  let startTime = findOldestEodBar dao securityId |> Option.map (fun bar -> bar.startTime)
  let endTime = findMostRecentEodBar dao securityId |> Option.map (fun bar -> bar.endTime)
  match (startTime, endTime) with
  | (Some startTime, Some endTime) -> Some <| intervalBetween startTime endTime
  | _ -> None

let priceHistoryContains dao (securityId: SecurityId) (interval: Interval): bool =
  priceHistoryInterval dao securityId
  |> Option.map (intervalsOverlap interval)
  |> Option.getOrElse false

let isEnoughPriceHistory dao (securityId: SecurityId) (tradingPeriodLength: Period): bool =
  let interval = priceHistoryInterval dao securityId
  match interval with
  | Some interval -> 
    let (tStart, tEnd) = (interval.Start.InZone(EasternTimeZone), interval.End.InZone(EasternTimeZone))
    let tradingStart = (tEnd.LocalDateTime - tradingPeriodLength).InZoneLeniently(EasternTimeZone)
    tStart <= tradingStart
  | None -> false

let securitiesWithEnoughPriceHistory dao (securityIds: seq<int>, tradingPeriodLength: Period): seq<int> =
  Seq.filter
    (fun securityId -> isEnoughPriceHistory dao securityId tradingPeriodLength)
    securityIds

(*
 * Returns the earliest and latest start-of-trading-period datetimes that the ticker represented by symbol may be traded,
 * given that an experiment may last for up to trading-period-length.
 * Usage: (trading-period-start-dates "intraday_data/AAPL.csv" (org.joda.time.Period/years 1))
 *        -> <#<ZonedDateTime 1999-04-01T08:32:00.000-06:00> #<ZonedDateTime 2008-04-01T13:42:00.000-05:00>>
 * For Reference: (price-history-start-end "intraday_data/AAPL.csv")
 *                -> <#<ZonedDateTime 1999-04-01T08:32:00.000-06:00> #<ZonedDateTime 2009-04-01T13:42:00.000-05:00>>
 *)
let tradingPeriodStartDates dao (securityId: SecurityId) (tradingPeriodLength: Period): Option<ZonedDateTime * ZonedDateTime> =
  priceHistoryInterval dao securityId
  |> Option.flatMap
    (fun interval ->
      let (tStart, tEnd) = (interval.Start.InZone(EasternTimeZone), interval.End.InZone(EasternTimeZone))
      let adjustedEnd = (tEnd.LocalDateTime - tradingPeriodLength).InZoneLeniently(EasternTimeZone)
      if adjustedEnd < tStart then
        None
      else
        Some (tStart, adjustedEnd)
    )

(*
 * Returns the common date range (CDR) of a set of price histories.
 * Example, we have the following price history for each of 3 companies.
 * Company A:                            |----------------------------------------------------------------------------|
 * Company B:                         |------------------------------------------------|
 * Company C:                                          |-------------------------------------------------------|
 * CDR (common date range):                            |-------------------------------|
 *
 * Returns an interval representing the start and end of the CDR.
 * If there is no common overlap among the companies, then the function returns nil.
 *
 * Example: (common-price-history-date-range <"AAPL", "F", "VFINX">)
 *          -> <#<ZonedDateTime 1987-03-27T09:30:00.000Z> #<ZonedDateTime 2012-10-10T16:00:00.000Z>>
 *)
let commonPriceHistoryDateRange dao (securityIds: seq<int>): Option<Interval> =
  let intervals = securityIds |> Seq.map (fun securityId -> priceHistoryInterval dao securityId)
  let iStart = intervals
               |> Seq.flatMapO
                 (Option.map (fun interval -> interval.Start))
               |> Seq.reduce
                 maxInstant   // get the latest (max) start date
  let iEnd = intervals
             |> Seq.flatMapO
               (Option.map (fun interval -> interval.End))
             |> Seq.reduce
               minInstant     // get the earliest (min) end date
  if iEnd < iStart then
    None
  else
    Some <| intervalBetweenInstants iStart iEnd

(*
 * Returns a pair of datetimes representing the earliest and latest dates that a trading strategy
 * may begin simultaneously trading a group of companies, assuming the trading strategy *may* trade the companies
 * for up to trading-period-length.
 *
 * Example, we have the following price history for each of 3 companies.
 * Company A:                            |----------------------------------------------------------------------------|
 * Company B:                         |------------------------------------------------|
 * Company C:                                          |-------------------------------------------------------|
 * CDR (common date range):                            |-------------------------------|
 * So, since the CDR (common date range) is the time period that we have price history information for all 3 companies
 * we can trade all 3 companies simultaneously during that time period ONLY if the time period is at least as long as
 * trading-period-length.
 *
 * This function returns a pair representing the earliest and latest start-of-trading-period datetimes that all companies
 * can be traded simultaneously for a period of trading-period-length.
 *
 * Usage: (common-trial-period-start-dates <"AAPL" "F"> (years 1))
 *        -> <#<ZonedDateTime 1984-09-07T09:30:00.000Z> #<ZonedDateTime 2011-10-10T16:00:00.000Z>>
 *)
let commonTrialPeriodStartDates dao (securityIds: seq<int>) (trialPeriodLength: Period): Option<Interval> =
  let intervals = securityIds |> Seq.map (fun securityId -> priceHistoryInterval dao securityId)
  let iStart = intervals
               |> Seq.flatMapO
                 (Option.map (fun interval -> interval.Start))
               |> Seq.reduce
                 maxInstant   // get the latest (max) start date
  let iEnd = intervals
             |> Seq.flatMapO
               (Option.map (fun interval -> interval.End))
             |> Seq.reduce
               minInstant     // get the earliest (min) end date
  let tStart = iStart.InZone(EasternTimeZone)
  let tEnd = iEnd.InZone(EasternTimeZone)
  let adjustedEnd = (tEnd.LocalDateTime - trialPeriodLength).InZoneLeniently(EasternTimeZone)
  if adjustedEnd < tStart then
    None
  else
    Some <| intervalBetween tStart adjustedEnd

let commonTrialPeriodStartDatesWithOffsets
    dao
    (securityIds: seq<int>)
    (trialPeriodLength: Period)
    (startOffsetDirection: Direction)
    (startOffset: Period)
    (endOffsetDirection: Direction)
    (endOffset: Period)
    : Option<Interval> =
  let getPriceHistoryInterval =
    (fun securityId -> priceHistoryInterval dao securityId) 
    >> Option.map (fun interval -> offsetInterval interval startOffsetDirection startOffset endOffsetDirection endOffset)
  let intervals = securityIds |> Seq.map getPriceHistoryInterval
  let iStart = intervals
               |> Seq.flatMapO
                 (Option.map (fun interval -> interval.Start))
               |> Seq.reduce
                 maxInstant   // get the latest (max) start date
  let iEnd = intervals
             |> Seq.flatMapO
               (Option.map (fun interval -> interval.End))
             |> Seq.reduce
               minInstant     // get the earliest (min) end date
  let tStart = iStart.InZone(EasternTimeZone)
  let tEnd = iEnd.InZone(EasternTimeZone)
  let adjustedEnd = (tEnd.LocalDateTime - trialPeriodLength).InZoneLeniently(EasternTimeZone)
  if adjustedEnd < tStart then
    None
  else
    Some <| intervalBetween tStart adjustedEnd
