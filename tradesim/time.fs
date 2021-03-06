﻿module dke.tradesim.Time

open System
open NodaTime
open NodaTime.Text

type timestamp = int64
type datestamp = int

module Period =
  let subtract (period: Period) (time: ZonedDateTime): ZonedDateTime = (time.LocalDateTime - period).InZoneLeniently(time.Zone)
  let add (period: Period) (time: ZonedDateTime): ZonedDateTime = (time.LocalDateTime + period).InZoneLeniently(time.Zone)


type DayOfWeek = 
  Monday = 1
  | Tuesday = 2
  | Wednesday = 3
  | Thursday = 4
  | Friday = 5
  | Saturday = 6
  | Sunday = 7

let toDayOfWeek = function
  1 -> DayOfWeek.Monday
  | 2 -> DayOfWeek.Tuesday
  | 3 -> DayOfWeek.Wednesday
  | 4 -> DayOfWeek.Thursday
  | 5 -> DayOfWeek.Friday
  | 6 -> DayOfWeek.Saturday
  | 7 -> DayOfWeek.Sunday
  | _ -> failwith "Day of week must be in range 1-7 inclusive."

let dayOfWeekToInt = function
  DayOfWeek.Monday -> 1
  | DayOfWeek.Tuesday -> 2
  | DayOfWeek.Wednesday -> 3
  | DayOfWeek.Thursday -> 4
  | DayOfWeek.Friday -> 5
  | DayOfWeek.Saturday -> 6
  | DayOfWeek.Sunday -> 7
  | _ -> failwith "Day of week must be in range 1-7 inclusive."

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
  let year = System.Convert.ToInt32(String.substring ts 0 4)
  let month = System.Convert.ToInt32(String.substring ts 4 2)
  let day = System.Convert.ToInt32(String.substring ts 6 2)
  let hour = System.Convert.ToInt32(String.substring ts 8 2)
  let minute = System.Convert.ToInt32(String.substring ts 10 2)
  let second = System.Convert.ToInt32(String.substring ts 12 2)
  datetime year month day hour minute second

let datestampToDatetime (datestamp: datestamp): ZonedDateTime =
  let ds = datestamp.ToString()
  let year = System.Convert.ToInt32(String.substring ds 0 4)
  let month = System.Convert.ToInt32(String.substring ds 4 2)
  let day = System.Convert.ToInt32(String.substring ds 6 2)
  datetime year month day 0 0 0

let localDateToDateTime (hour: int) (minute: int) (second: int) (date: LocalDate): ZonedDateTime =
  datetime date.Year date.Month date.Day hour minute second

let timestampPattern = ZonedDateTimePattern.CreateWithInvariantCulture("yyyyMMddHHmmss", DateTimeZoneProviders.Tzdb)
let dateTimeToTimestampStr (time: ZonedDateTime): string = timestampPattern.Format(time)
let dateTimeToTimestamp (time: ZonedDateTime): timestamp = Int64.Parse(dateTimeToTimestampStr time)
let zonedDatestampPattern = ZonedDateTimePattern.CreateWithInvariantCulture("yyyyMMdd", DateTimeZoneProviders.Tzdb)
let dateTimeToDatestampStr (time: ZonedDateTime): string = zonedDatestampPattern.Format(time)
let dateTimeToDatestamp (time: ZonedDateTime): datestamp = Int32.Parse(dateTimeToDatestampStr time)

let date (year: int) (month: int) (day: int) = new LocalDate(year, month, day)

let dateTimeToLocalDate (time: ZonedDateTime): LocalDate = date time.Year time.Month time.Day

let timestampToDate (timestamp: timestamp): LocalDate = (timestamp |> timestampToDatetime).Date

let datestampToDate (datestamp: datestamp): LocalDate =
  let ds = datestamp.ToString()
  let year = System.Convert.ToInt32(String.substring ds 0 4)
  let month = System.Convert.ToInt32(String.substring ds 4 2)
  let day = System.Convert.ToInt32(String.substring ds 6 2)
  date year month day

let datestampPattern = LocalDatePattern.CreateWithInvariantCulture("yyyyMMdd")
let localDateToDatestampStr (date: LocalDate): string = datestampPattern.Format(date)
let localDateToDatestamp (date: LocalDate): datestamp = Int32.Parse(localDateToDatestampStr date)

let years n: Period = Period.FromYears(n)
let months n: Period = Period.FromMonths(n)
let weeks n: Period = Period.FromWeeks(n)
let days n: Period = Period.FromDays(n)
let hours n: Period = Period.FromHours(n)
let minutes n: Period = Period.FromMinutes(n)
let seconds n: Period = Period.FromSeconds(n)

let hoursD n: Duration = Duration.FromHours(n)
let minutesD n: Duration = Duration.FromMinutes(n)
let secondsD n: Duration = Duration.FromSeconds(n)
let millisD n: Duration = Duration.FromMilliseconds(n)

let midnightOnDate = localDateToDateTime 0 0 0
let midnight (time: ZonedDateTime): ZonedDateTime = datetime time.Year time.Month time.Day 0 0 0

let compareDateTimes (t1: ZonedDateTime) (t2: ZonedDateTime): int = t1.CompareTo(t2)

let periodBetween (t1: ZonedDateTime) (t2: ZonedDateTime): Period = Period.Between(t1.LocalDateTime, t2.LocalDateTime)

let durationBetween (t1: ZonedDateTime) (t2: ZonedDateTime): Duration = t1.ToInstant() - t2.ToInstant()

let intervalBetweenInstants (i1: Instant) (i2: Instant): Interval = new Interval(i1, i2)
let intervalBetween (t1: ZonedDateTime) (t2: ZonedDateTime): Interval = intervalBetweenInstants <| t1.ToInstant() <| t2.ToInstant()

let intervalsOverlap (i1: Interval) (i2: Interval): bool = i1.Contains(i2.Start) || i1.Contains(i2.End) || (i2.Start < i1.Start && i1.End <= i2.End)

// todo: ensure that this is always going to return the proper values: e.g. 1 week should be represented as P1W, and not P7D
let formatPeriod (period: Period): string = PeriodPattern.RoundtripPattern.Format(period)

let parsePeriod (period: string): Option<Period> = 
  let parseResult = PeriodPattern.RoundtripPattern.Parse(period)
  if parseResult.Success then
    Some parseResult.Value
  else
    None

// t1 <= instant < t2
let isInstantBetween (instant: ZonedDateTime) (t1: ZonedDateTime) (t2: ZonedDateTime): bool = t1 <= instant && instant < t2

// t1 <= instant <= t2
let isInstantBetweenInclusive (instant: ZonedDateTime) (t1: ZonedDateTime) (t2: ZonedDateTime): bool = t1 <= instant && instant <= t2

let maxInstant (i1: Instant) (i2: Instant): Instant = if (i1 > i2) then i1 else i2
let maxDatetime (t1: ZonedDateTime) (t2: ZonedDateTime): ZonedDateTime = if (t1 > t2) then t1 else t2

let minInstant (i1: Instant) (i2: Instant): Instant = if (i1 < i2) then i1 else i2
let minDatetime (t1: ZonedDateTime) (t2: ZonedDateTime): ZonedDateTime = if (t1 < t2) then t1 else t2

type Direction = Before | After
let offsetDateTime (t: ZonedDateTime) (direction: Direction) (magnitude: Period): ZonedDateTime = 
  match direction with
  | Before -> t |> Period.subtract magnitude
  | After -> t |> Period.add magnitude

let offsetInterval (startOffsetDirection: Direction)
                   (startOffsetMagnitude: Period)
                   (endOffsetDirection: Direction)
                   (endOffsetMagnitude: Period)
                   (interval: Interval)
                   : Interval =
  let adjustedStart = offsetDateTime (interval.Start |> instantToZonedTime EasternTimeZone) startOffsetDirection startOffsetMagnitude
  let adjustedEnd = offsetDateTime (interval.End |> instantToZonedTime EasternTimeZone) endOffsetDirection endOffsetMagnitude
  intervalBetween adjustedStart adjustedEnd

// returns an infinite seq of [t f(t) f(f(t)) f(f(f(t))) ...]
let timeSeries (startTime: ZonedDateTime) (nextTimeFn: ZonedDateTime -> ZonedDateTime): seq<ZonedDateTime> = Seq.iterate nextTimeFn startTime

// returns an infinite seq of [d f(d) f(f(d)) f(f(f(d))) ...]
let dateSeries (startDate: LocalDate) (nextDateFn: LocalDate -> LocalDate): seq<LocalDate> = Seq.iterate nextDateFn startDate

// returns an infinite seq of [t t+p t+2p t+3p ...]
let infPeriodicalTimeSeries (startTime: ZonedDateTime) (period: Period): seq<ZonedDateTime> = timeSeries startTime (Period.add period)

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
  Seq.map (fun t -> intervalBetween t (Period.add intervalLength t)) startTimes

let interspersedIntervals2 (firstIntervalStartTime: ZonedDateTime) (lastIntervalStartTime: ZonedDateTime) (intervalLength: Period) (separationLength: Period): seq<Interval> =
  let startTimes = infPeriodicalTimeSeries firstIntervalStartTime separationLength |> Seq.takeWhile (fun t -> t <= lastIntervalStartTime)
  startTimes |> Seq.map (fun t -> intervalBetween t (Period.add intervalLength t))

let interspersedIntervals (startTimeInterval: Interval) (intervalLength: Period) (separationLength: Period): seq<Interval> =
  let startTime = startTimeInterval.Start |> instantToZonedTime EasternTimeZone
  let endTime = startTimeInterval.End |> instantToZonedTime EasternTimeZone
  interspersedIntervals2 startTime endTime intervalLength separationLength

let daysInMonth (year: int) (month: int): int = DateTime.DaysInMonth(year, month)

// returns [month, year] representing the month and year following the given month and year
let nextMonth month year = 
  if month = 12 then
    (1, year + 1)
  else
    (month + 1, year)

// returns [month, year] representing the month and year preceeding the given month and year
let previousMonth month year = 
  if month = 1 then
    (12, year - 1)
  else
    (month - 1, year)

let addMonths baseMonth baseYear monthOffset =
  if monthOffset >= 0 then
    seq { 1..monthOffset }
    |> Seq.fold
      (fun (month, year) i -> nextMonth month year)
      (baseMonth, baseYear)
  else
    seq { 1..(-monthOffset) }
    |> Seq.fold
      (fun (month, year) i -> previousMonth month year)
      (baseMonth, baseYear)

let firstDayOfMonth (year: int) (month: int): LocalDate = date year month 1

let lastDayOfMonth (year: int) (month: int): LocalDate = date year month <| daysInMonth year month

let nextDay (date: LocalDate): LocalDate = date + days 1L
let previousDay (date: LocalDate): LocalDate = date - days 1L

// returns day of week where 1 = Monday, ..., 7 = Sunday
let dayOfWeek (t: ZonedDateTime): DayOfWeek = t.DayOfWeek |> toDayOfWeek

let dayOfWeekD (date: LocalDate): DayOfWeek = date.DayOfWeek |> toDayOfWeek
let isDateMonday date = dayOfWeekD date = DayOfWeek.Monday
let isDateTuesday date = dayOfWeekD date = DayOfWeek.Tuesday
let isDateWednesday date = dayOfWeekD date = DayOfWeek.Wednesday
let isDateThursday date = dayOfWeekD date = DayOfWeek.Thursday
let isDateFriday date = dayOfWeekD date = DayOfWeek.Friday
let isDateSaturday date = dayOfWeekD date = DayOfWeek.Saturday
let isDateSunday date = dayOfWeekD date = DayOfWeek.Sunday

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
let offsetOfFirstWeekdayInMonth (desiredWeekday: DayOfWeek) (month: int) (year: int): int =
  (dayOfWeekToInt desiredWeekday - (datetime year month 1 0 0 0 |> dayOfWeek |> dayOfWeekToInt) + 7) % 7

(*
 * The return value will be an integer in the range [0, 6].
 * desired_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * current_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 *)
let offsetOfFirstWeekdayAtOrAfterWeekday (desiredWeekday: DayOfWeek) (currentWeekday: DayOfWeek): int = 
  (dayOfWeekToInt desiredWeekday - dayOfWeekToInt currentWeekday + 7) % 7

(*
 * The return value will be an integer in the range [-6, 0].
 * desired_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * current_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 *)
let offsetOfFirstWeekdayAtOrBeforeWeekday (desiredWeekday: DayOfWeek) (currentWeekday: DayOfWeek): int = 
  -((dayOfWeekToInt currentWeekday - dayOfWeekToInt desiredWeekday + 7) % 7)

(*
 * The return value will be an integer in the range [1, 7].
 * desired_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * current_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * Example:
 * offset_of_first_weekday_after_weekday(2, 2) => 7
 * offset_of_first_weekday_after_weekday(5, 2) => 3
 * offset_of_first_weekday_after_weekday(3, 6) => 4
 *)
let offsetOfFirstWeekdayAfterWeekday (desiredWeekday: DayOfWeek) (currentWeekday: DayOfWeek): int =
  let offset = offsetOfFirstWeekdayAtOrAfterWeekday desiredWeekday currentWeekday
  if offset = 0 then 7
  else offset

(*
 * The return value will be an integer in the range [-7, -1].
 * desired_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * current_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * Example:
 * offset_of_first_weekday_before_weekday(2, 2) => -7
 * offset_of_first_weekday_before_weekday(5, 2) => -4
 * offset_of_first_weekday_before_weekday(3, 6) => -3
 *)
let offsetOfFirstWeekdayBeforeWeekday (desiredWeekday: DayOfWeek) (currentWeekday: DayOfWeek): int =
  let offset = offsetOfFirstWeekdayAtOrBeforeWeekday desiredWeekday currentWeekday
  if offset = 0 then -7
  else offset

(*
 * desired_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * Example:
 * first_weekday_after_date(DayOfWeek::Friday, Date.new(2012, 2, 18)) => #<Date: 2012-02-24 ((2455982j,0s,0n),+0s,2299161j)>
 * first_weekday_after_date(DayOfWeek::Friday, Date.new(2012, 2, 24)) => #<Date: 2012-03-02 ((2455989j,0s,0n),+0s,2299161j)>
 *)
let firstWeekdayAfterDate (desiredWeekday: DayOfWeek) (date: LocalDate): LocalDate =
  let offset = offsetOfFirstWeekdayAfterWeekday desiredWeekday <| (date |> dayOfWeekD)
  date + (offset |> int64 |> days)

(*
 * desired_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * Example:
 * first_weekday_at_or_after_date(DayOfWeek::Friday, Date.new(2012, 2, 18)) => #<Date: 2012-02-24 ((2455982j,0s,0n),+0s,2299161j)>
 * first_weekday_at_or_after_date(DayOfWeek::Friday, Date.new(2012, 2, 24)) => #<Date: 2012-02-24 ((2455982j,0s,0n),+0s,2299161j)>
 *)
let firstWeekdayAtOrAfterDate (desiredWeekday: DayOfWeek) (date: LocalDate): LocalDate =
  let offset = offsetOfFirstWeekdayAtOrAfterWeekday desiredWeekday <| dayOfWeekD date
  date + (offset |> int64 |> days)

(*
 * desired_weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * Example:
 * first_weekday_before_date(DayOfWeek::Friday, Date.new(2012, 3, 2)) => #<Date: 2012-02-24 ((2455982j,0s,0n),+0s,2299161j)>
 * first_weekday_before_date(DayOfWeek::Wednesday, Date.new(2012, 3, 2)) => #<Date: 2012-02-29 ((2455987j,0s,0n),+0s,2299161j)>
 *)
let firstWeekdayBeforeDate (desiredWeekday: DayOfWeek) (date: LocalDate): LocalDate =
  let offset = offsetOfFirstWeekdayBeforeWeekday desiredWeekday <| dayOfWeekD date
  date + (offset |> int64 |> days)

let firstWeekdayAtOrBeforeDate (desiredWeekday: DayOfWeek) (date: LocalDate): LocalDate =
  let offset = offsetOfFirstWeekdayAtOrBeforeWeekday desiredWeekday <| dayOfWeekD date
  date + (offset |> int64 |> days)

(*
 * returns a Date representing the nth weekday after the given date
 * desired-weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * Example:
 * nth_weekday_at_or_after_date(1, DayOfWeek::Friday, Date.new(2012, 2, 3)) => #<Date: 2012-02-03 ((2455961j,0s,0n),+0s,2299161j)>
 * nth_weekday_at_or_after_date(2, DayOfWeek::Friday, Date.new(2012, 2, 3)) => #<Date: 2012-02-10 ((2455968j,0s,0n),+0s,2299161j)>
 *)
let nthWeekdayAtOrAfterDate (n: int) (desiredWeekday: DayOfWeek) (date: LocalDate): LocalDate =
  let weekOffsetInDays = 7 * (n - 1) |> int64 |> days
  firstWeekdayAtOrAfterDate desiredWeekday date + weekOffsetInDays

let nthWeekdayAtOrBeforeDate (n: int) (desiredWeekday: DayOfWeek) (date: LocalDate): LocalDate =
  let weekOffsetInDays = 7 * (n - 1) |> int64 |> days
  firstWeekdayAtOrBeforeDate desiredWeekday date - weekOffsetInDays

(*
 * returns a Date representing the nth weekday after the given date
 * desired-weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * Example:
 * nth_weekday_after_date(1, DayOfWeek::Friday, Date.new(2012, 2, 18)) => #<Date: 2012-02-24 ((2455982j,0s,0n),+0s,2299161j)>
 * nth_weekday_after_date(2, DayOfWeek::Friday, Date.new(2012, 2, 18)) => #<Date: 2012-03-02 ((2455989j,0s,0n),+0s,2299161j)>
 * nth_weekday_after_date(4, DayOfWeek::Wednesday, Date.new(2012, 2, 18)) => #<Date: 2012-03-14 ((2456001j,0s,0n),+0s,2299161j)>
 *)
let nthWeekdayAfterDate (n: int) (desiredWeekday: DayOfWeek) (date: LocalDate): LocalDate =
  let weekOffsetInDays = 7 * (n - 1) |> int64 |> days
  firstWeekdayAfterDate desiredWeekday date + weekOffsetInDays

(*
 * returns a Date representing the nth weekday after the given date
 * desired-weekday is an integer indicating the desired day of the week, s.t. 1=Monday, 2=Tue., ..., 6=Sat., 7=Sun.
 * Example:
 * nth_weekday_before_date(1, DayOfWeek::Friday, Date.new(2012, 3, 2)) => #<Date: 2012-02-24 ((2455982j,0s,0n),+0s,2299161j)>
 * nth_weekday_before_date(2, DayOfWeek::Friday, Date.new(2012, 3, 2)) => #<Date: 2012-02-17 ((2455975j,0s,0n),+0s,2299161j)>
 * nth_weekday_before_date(4, DayOfWeek::Wednesday, Date.new(2012, 3, 2)) => #<Date: 2012-02-08 ((2455966j,0s,0n),+0s,2299161j)>
 *)
let nthWeekdayBeforeDate (n: int) (desiredWeekday: DayOfWeek) (date: LocalDate): LocalDate =
  let weekOffsetInDays = 7 * (n - 1) |> int64 |> days
  firstWeekdayBeforeDate desiredWeekday date - weekOffsetInDays

(*
 * returns a LocalDate representing the nth weekday in the given month.
 * Example:
 *   nthWeekdayOfMonth(3, DateTimeConstants.MONDAY, 1, 2012)   ; returns the 3rd monday in January 2012.
 *   => #<LocalDate 2012-01-16>
 *   nthWeekdayOfMonth(3, DateTimeConstants.MONDAY, 2, 2012)   ; returns the 3rd monday in February 2012.
 *   => #<LocalDate 2012-02-20>
 *)
let nthWeekdayOfMonth (n: int) (desiredWeekday: DayOfWeek) (month: Month) (year: int): LocalDate = 
//  let firstDayOfTheMonth = date year (int month) 1
//  let firstDesiredWeekdayOfTheMonth = firstDayOfTheMonth + Period.FromDays(offsetOfFirstWeekdayInMonth (int desiredWeekday) (int month) year |> int64)
//  let weekOffsetInDays = Period.FromDays(7 * (n - 1) |> int64)
//  firstDesiredWeekdayOfTheMonth + weekOffsetInDays
  nthWeekdayAtOrAfterDate n desiredWeekday <| firstDayOfMonth year (int month)

(*
 * Returns a LocalDate representing the last weekday in the given month.
 * source: http://www.irt.org/articles/js050/
 * formula:
 *   daysInMonth - (DayOfWeek(daysInMonth,month,year) - desiredWeekday + 7)%7
 * Example:
 *   lastWeekdayOfMonth DayOfWeek.Monday Month.February 2012;;
 *   val it : NodaTime.LocalDate = Monday, February 27, 2012 {...}
 *)
let lastWeekdayOfMonth (desiredWeekday: DayOfWeek) (month: Month) (year: int): LocalDate = 
  let month' = int month
  let days = daysInMonth year month'
  let dayOfMonth = days - ((datetime year month' days 0 0 0 |> dayOfWeek |> dayOfWeekToInt) - (desiredWeekday |> dayOfWeekToInt) + 7) % 7
  date year month' dayOfMonth

let firstMondayAfter = firstWeekdayAfterDate DayOfWeek.Monday
let firstMondayAtOrAfter = firstWeekdayAtOrAfterDate DayOfWeek.Monday

let firstMondayBefore = firstWeekdayBeforeDate DayOfWeek.Monday
let firstMondayAtOrBefore = firstWeekdayAtOrBeforeDate DayOfWeek.Monday

let previousBusinessDay (date: LocalDate) =
  if dayOfWeekD date = DayOfWeek.Monday then date - (days 3L)
  else date - (days 1L)

let nextBusinessDay (date: LocalDate) =
  if dayOfWeekD date = DayOfWeek.Friday then date + (days 3L)
  else date + (days 1L)

let isBusinessDay date = dayOfWeekD date < DayOfWeek.Saturday
