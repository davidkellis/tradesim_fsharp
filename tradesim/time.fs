module dke.tradesim.Time

open NodaTime

type timestamp = int64
type datestamp = int


let findTimeZone (timeZoneName: string): DateTimeZone = DateTimeZoneProviders.Tzdb.[timeZoneName]

// found the names of the relevant tzdb time zones here: http://en.wikipedia.org/wiki/List_of_tz_database_time_zones
let EasternTimeZone = findTimeZone "America/New_York"
let CentralTimeZone = findTimeZone "America/Chicago"

let currentTime (timeZone: DateTimeZone option): ZonedDateTime = 
  let tz = defaultArg timeZone EasternTimeZone
  new ZonedDateTime(SystemClock.Instance.Now, tz)

let datetime (year: int), (month: int), (day: int), (hour: int), (minute: int), (second: int): ZonedDateTime =
  new ZonedDateTime(year, month, day, hour, minute, second, EasternTimeZone)
let datetime (timestamp: timestamp): ZonedDateTime = 
  let ts = timestamp.ToString()
  let year = System.Convert.ToInt32(ts.Substring(0, 4))
  let month = System.Convert.ToInt32(ts.Substring(4, 6))
  let day = System.Convert.ToInt32(ts.Substring(6, 8))
  let hour = System.Convert.ToInt32(ts.Substring(8, 10))
  let minute = System.Convert.ToInt32(ts.Substring(10, 12))
  let second = System.Convert.ToInt32(ts.Substring(12, 14))
  datetime(year, month, day, hour, minute, second)


let date (time: ZonedDateTime): LocalDate = new LocalDate(time.Year, time.Month, time.Day)
let date (timestamp: timestamp): LocalDate = date(datetime(timestamp))


