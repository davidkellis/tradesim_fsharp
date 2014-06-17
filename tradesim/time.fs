module dke.tradesim.Time

open System
open NodaTime
open NodaTime.Text

type timestamp = int64
type datestamp = int

type DayOfWeek = 
  Monday = 1
  | Tuesday = 2
  | Wednesday = 3
  | Thursday = 4
  | Friday = 5
  | Saturday = 6
  | Sunday = 7

type Month = 
  January = 1
  | February = 2
  | March = 3
  | April = 4
  | May = 5
  | June = 6
  | July = 7
  | August = 8
  | September = 9
  | October = 10
  | November = 11
  | December = 12

let findTimeZone (timeZoneName: string): DateTimeZone = DateTimeZoneProviders.Tzdb.[timeZoneName]

// found the names of the relevant tzdb time zones here: http://en.wikipedia.org/wiki/List_of_tz_database_time_zones
let EasternTimeZone = findTimeZone "America/New_York"
//let CentralTimeZone = findTimeZone "America/Chicago"

let toZonedTime (timeZone: DateTimeZone) (time: LocalDateTime) = timeZone.AtLeniently(time)
let toEasternTime = toZonedTime EasternTimeZone

let instantToZonedTime (timeZone: DateTimeZone) (instant: Instant) = instant.InZone(timeZone)
let instantToEasternTime = instantToZonedTime EasternTimeZone

let currentTime (timeZone: DateTimeZone option): ZonedDateTime = 
  let tz = defaultArg timeZone EasternTimeZone
  new ZonedDateTime(SystemClock.Instance.Now, tz)

let datetime (year: int) (month: int) (day: int) (hour: int) (minute: int) (second: int): ZonedDateTime =
  (new LocalDateTime(year, month, day, hour, minute, second)).InZoneLeniently(EasternTimeZone)

let timestampToDatetime (timestamp: timestamp): ZonedDateTime = 
  let ts = timestamp.ToString()
  let year = System.Convert.ToInt32(ts.Substring(0, 4))
  let month = System.Convert.ToInt32(ts.Substring(4, 6))
  let day = System.Convert.ToInt32(ts.Substring(6, 8))
  let hour = System.Convert.ToInt32(ts.Substring(8, 10))
  let minute = System.Convert.ToInt32(ts.Substring(10, 12))
  let second = System.Convert.ToInt32(ts.Substring(12, 14))
  datetime year month day hour minute second

let datestampToDatetime (datestamp: datestamp): ZonedDateTime =
  let ds = datestamp.ToString()
  let year = System.Convert.ToInt32(ds.Substring(0, 4))
  let month = System.Convert.ToInt32(ds.Substring(4, 6))
  let day = System.Convert.ToInt32(ds.Substring(6, 8))
  datetime year month day 0 0 0

let localDateToDateTime (date: LocalDate) (hour: int) (minute: int) (second: int): ZonedDateTime =
  datetime date.Year date.Month date.Day hour minute second

let timestampPattern = ZonedDateTimePattern.CreateWithInvariantCulture("yyyyMMddHHmmss", DateTimeZoneProviders.Tzdb)
let dateTimeToTimestampStr (time: ZonedDateTime): string = timestampPattern.Format(time)
let dateTimeToTimestamp (time: ZonedDateTime): timestamp = Int64.Parse(dateTimeToTimestampStr time)

let date (year: int) (month: int) (day: int) = new LocalDate(year, month, day)

let timestampToDate (timestamp: timestamp): LocalDate = (timestamp |> timestampToDatetime).Date

let datestampToDate (datestamp: datestamp): LocalDate =
  let ds = datestamp.ToString()
  let year = System.Convert.ToInt32(ds.Substring(0, 4))
  let month = System.Convert.ToInt32(ds.Substring(4, 6))
  let day = System.Convert.ToInt32(ds.Substring(6, 8))
  date year month day

let years n: Period = Period.FromYears(n)
let days n: Period = Period.FromDays(n)
let hours n: Period = Period.FromHours(n)
let seconds n: Period = Period.FromSeconds(n)

let compareDateTimes (t1: ZonedDateTime) (t2: ZonedDateTime): int = t1.CompareTo(t2)

let periodBetween (t1: ZonedDateTime) (t2: ZonedDateTime): Period = Period.Between(t1.LocalDateTime, t2.LocalDateTime)

let durationBetween (t1: ZonedDateTime) (t2: ZonedDateTime): Duration = t1.ToInstant() - t2.ToInstant()

let intervalBetween (t1: ZonedDateTime) (t2: ZonedDateTime): Interval = new Interval(t1.ToInstant(), t2.ToInstant())

let intervalsOverlap (i1: Interval) (i2: Interval): bool = i1.Contains(i2.Start) || i1.Contains(i2.End) || (i2.Start < i1.Start && i1.End <= i2.End)

let prettyFormatPeriod (period: Period): string = period.ToString()

// t1 <= instant < t2
let isInstantBetween (instant: ZonedDateTime) (t1: ZonedDateTime) (t2: ZonedDateTime): bool = t1 <= instant && instant < t2

// t1 <= instant <= t2
let isInstantBetweenInclusive (instant: ZonedDateTime) (t1: ZonedDateTime) (t2: ZonedDateTime): bool = t1 <= instant && instant <= t2

let maxDatetime (t1: ZonedDateTime) (t2: ZonedDateTime): ZonedDateTime = if (t1 > t2) then t1 else t2

let minDatetime (t1: ZonedDateTime) (t2: ZonedDateTime): ZonedDateTime = if (t1 < t2) then t1 else t2

type Direction = Before | After
let offsetDateTime (t: ZonedDateTime) (direction: Direction) (magnitude: Period): ZonedDateTime = 
  match direction with
  | Before -> t.Minus(magnitude.ToDuration())
  | After -> t.Plus(magnitude.ToDuration())

let offsetInterval (interval: Interval)
                   (startOffsetDirection: Direction)
                   (startOffsetMagnitude: Period)
                   (endOffsetDirection: Direction)
                   (endOffsetMagnitude: Period): Interval =
  let adjustedStart = offsetDateTime (interval.Start.InZone(EasternTimeZone)) startOffsetDirection startOffsetMagnitude
  let adjustedEnd = offsetDateTime (interval.End.InZone(EasternTimeZone)) endOffsetDirection endOffsetMagnitude
  intervalBetween adjustedStart adjustedEnd

// returns an infinite seq of [t f(t) f(f(t)) f(f(f(t))) ...]
let timeSeries (startTime: ZonedDateTime) (nextTimeFn: ZonedDateTime -> ZonedDateTime): seq<ZonedDateTime> = Seq.iterate nextTimeFn startTime

// returns an infinite seq of [d f(d) f(f(d)) f(f(f(d))) ...]
let dateSeries (startDate: LocalDate) (nextDateFn: LocalDate -> LocalDate): seq<LocalDate> = Seq.iterate nextDateFn startDate

// returns an infinite seq of [t t+p t+2p t+3p ...]
let infPeriodicalTimeSeries (startTime: ZonedDateTime) (period: Period): seq<ZonedDateTime> = timeSeries startTime (fun t -> t + period.ToDuration())

// returns an infinite seq of [d d+p d+2p d+3p ...]
let infPeriodicalDateSeries (startDate: LocalDate) (period: Period): seq<LocalDate> = dateSeries startDate (fun d -> d + period)

// returns a sequence of DateTimes, [t1, t2, ..., tN], that are separated by a given Period, s.t. startTime = t1 and tN <= endTime
let periodicalTimeSeries (startTime: ZonedDateTime) (endTime: ZonedDateTime) (period: Period): seq<ZonedDateTime> =
  infPeriodicalTimeSeries startTime period |> Seq.takeWhile (fun t -> t <= endTime)

let periodicalDateSeries (startDate: LocalDate) (endDate: LocalDate) (period: Period): seq<LocalDate> =
  infPeriodicalDateSeries startDate period |> Seq.takeWhile (fun t -> t <= endDate)

// returns an infinite sequence
// returns a sequence of Intervals, [i1, i2, ..., iN], s.t. the start time of subsequent intervals is separated
// by a given Period, <separationLength> and each interval spans an amount of time given by <intervalLength>
let infInterspersedIntervals (startTime: ZonedDateTime) (intervalLength: Period) (separationLength: Period): seq<Interval> =
  let startTimes = infPeriodicalTimeSeries startTime separationLength
  let duration = intervalLength.ToDuration()
  Seq.map (fun t -> intervalBetween t (t + duration)) startTimes

let interspersedIntervals (startTimeInterval: Interval) (intervalLength: Period) (separationLength: Period): seq<Interval> =
  let startTime = startTimeInterval.Start.InZone(EasternTimeZone)
  let endTime = startTimeInterval.End.InZone(EasternTimeZone)
  let startTimes = infPeriodicalTimeSeries startTime separationLength |> Seq.takeWhile (fun t -> t <= endTime)
  let duration = intervalLength.ToDuration()
  Seq.map (fun t -> intervalBetween t (t + duration)) startTimes

let daysInMonth (year: int) (month: int): int = DateTime.DaysInMonth(year, month)

let dayOfWeek(t: ZonedDateTime): int = t.DayOfWeek

(*
 * Returns the number of days that must be added to the first day of the given month to arrive at the first
 *   occurrence of the <desired-weekday> in that month; put another way, it returns the number of days
 *   that must be added to the first day of the given month to arrive at the <desired-weekday> in the first
 *   week of that month.
 * The return value will be an integer in the range [0, 6].
 * NOTE: the return value is the result of the following expression:
 *   (desired-weekday - dayOfWeek(year, month, 1) + 7) mod 7
 * desired-weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * month is an integer indicating the month, s.t. 1=Jan., 2=Feb., ..., 11=Nov., 12=Dec.
 * year is an integer indicating the year (e.g. 1999, 2010, 2012, etc.)
 * Example:
 *   offsetOfFirstWeekdayInMonth(1, 2, 2012)    ; monday
 *   > 5
 *   offsetOfFirstWeekdayInMonth(3, 2, 2012)    ; wednesday
 *   > 0
 *   offsetOfFirstWeekdayInMonth(5, 2, 2012)    ; friday
 *   > 2
 *)
let offsetOfFirstWeekdayInMonth (desiredWeekday: int) (month: int) (year: int): int =
  (desiredWeekday - (dayOfWeek <| datetime year month 1 0 0 0) + 7) % 7

(*
 * returns a LocalDate representing the nth weekday in the given month.
 * Example:
 *   nthWeekday(3, DateTimeConstants.MONDAY, 1, 2012)   ; returns the 3rd monday in January 2012.
 *   => #<LocalDate 2012-01-16>
 *   nthWeekday(3, DateTimeConstants.MONDAY, 2, 2012)   ; returns the 3rd monday in February 2012.
 *   => #<LocalDate 2012-02-20>
 *)
let nthWeekday (n: int) (desiredWeekday: DayOfWeek) (month: Month) (year: int): LocalDate = 
  let firstDayOfTheMonth = date year (int month) 1
  let firstDesiredWeekdayOfTheMonth = firstDayOfTheMonth + Period.FromDays(offsetOfFirstWeekdayInMonth (int desiredWeekday) (int month) year |> int64)
  let weekOffsetInDays = Period.FromDays(7 * (n - 1) |> int64)
  firstDesiredWeekdayOfTheMonth + weekOffsetInDays

(*
 * Returns a LocalDate representing the last weekday in the given month.
 * source: http://www.irt.org/articles/js050/
 * formula:
 *   daysInMonth - (DayOfWeek(daysInMonth,month,year) - desiredWeekday + 7)%7
 * Example:
 *   lastWeekday DayOfWeek.Monday Month.February 2012;;
 *   val it : NodaTime.LocalDate = Monday, February 27, 2012 {...}
 *)
let lastWeekday (desiredWeekday: DayOfWeek) (month: Month) (year: int): LocalDate = 
  let month' = int month
  let days = daysInMonth year month'
  let dayOfMonth = days - ((datetime year month' days 0 0 0 |> dayOfWeek) - (int desiredWeekday) + 7) % 7
  date year month' dayOfMonth
