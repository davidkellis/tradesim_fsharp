module dke.tradesim.Time

open NodaTime

type timestamp = int64
type datestamp = int


let findTimeZone (timeZoneName: string): DateTimeZone = DateTimeZoneProviders.Tzdb.[timeZoneName]

// found the names of the relevant tzdb time zones here: http://en.wikipedia.org/wiki/List_of_tz_database_time_zones
let EasternTimeZone = findTimeZone "America/New_York"
//let CentralTimeZone = findTimeZone "America/Chicago"

let toZonedTime (timeZone: DateTimeZone) (time: LocalDateTime) = timeZone.AtLeniently(time)
let toEasternTime = toZonedTime EasternTimeZone

let currentTime (timeZone: DateTimeZone option): ZonedDateTime = 
  let tz = defaultArg timeZone EasternTimeZone
  new ZonedDateTime(SystemClock.Instance.Now, tz)

let datetime (year: int) (month: int) (day: int) (hour: int) (minute: int) (second: int): LocalDateTime =
  new LocalDateTime(year, month, day, hour, minute, second)

let timestampToDatetime (timestamp: timestamp): LocalDateTime = 
  let ts = timestamp.ToString()
  let year = System.Convert.ToInt32(ts.Substring(0, 4))
  let month = System.Convert.ToInt32(ts.Substring(4, 6))
  let day = System.Convert.ToInt32(ts.Substring(6, 8))
  let hour = System.Convert.ToInt32(ts.Substring(8, 10))
  let minute = System.Convert.ToInt32(ts.Substring(10, 12))
  let second = System.Convert.ToInt32(ts.Substring(12, 14))
  datetime year month day hour minute second

let datestampToDatetime (datestamp: datestamp): LocalDateTime =
  let ds = datestamp.ToString()
  let year = System.Convert.ToInt32(ds.Substring(0, 4))
  let month = System.Convert.ToInt32(ds.Substring(4, 6))
  let day = System.Convert.ToInt32(ds.Substring(6, 8))
  datetime year month day 0 0 0

let date (year: int) (month: int) (day: int) = new LocalDate(year, month, day)
//let date (time: LocalDateTime): LocalDate = time.Date

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

let periodBetween (t1: LocalDateTime) (t2: LocalDateTime): Period = Period.Between(t1, t2)

let durationBetween (t1: ZonedDateTime) (t2: ZonedDateTime): Duration = t1.ToInstant() - t2.ToInstant()

let intervalBetween (t1: ZonedDateTime) (t2: ZonedDateTime): Interval = new Interval(t1.ToInstant(), t2.ToInstant())

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

// returns an infinite sequence
let timeSeries (startTime: ZonedDateTime) (nextTimeFn: ZonedDateTime -> ZonedDateTime): seq<ZonedDateTime> = Seq.iterate nextTimeFn startTime
