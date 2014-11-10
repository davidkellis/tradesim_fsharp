module dke.tradesim.Schedule

open C5
open NodaTime
open FSharpx
open System.Collections.Generic

open Stdlib
open Cache
open Time
open MInterval
open SpecialDates


type TradingSchedule = LocalDate -> MInterval

let firstTradingDay = date 1950 1 1
let lastTradingDay = date 2050 1 1
let defaultStartOfTrading = new LocalTime(9, 30, 0)   // Eastern Time
let defaultEndOfTrading = new LocalTime(16, 0, 0)     // Eastern Time
let defaultDailyTradingHours = (defaultStartOfTrading, defaultEndOfTrading)

let defaultWeeklyTradingHours = Map.ofList [
                                             DayOfWeek.Monday |> dayOfWeekToInt, defaultDailyTradingHours;
                                             DayOfWeek.Tuesday |> dayOfWeekToInt, defaultDailyTradingHours;
                                             DayOfWeek.Wednesday |> dayOfWeekToInt, defaultDailyTradingHours;
                                             DayOfWeek.Thursday |> dayOfWeekToInt, defaultDailyTradingHours;
                                             DayOfWeek.Friday |> dayOfWeekToInt, defaultDailyTradingHours
                                           ]

let defaultTradingSchedule (date: LocalDate): MInterval =
  let tradingHours = Map.tryFind date.DayOfWeek defaultWeeklyTradingHours
  tradingHours
  |> Option.map 
    (fun (startOfTrading, endOfTrading) ->
      let startOfTradingDateTime = localDateToDateTime startOfTrading.Hour startOfTrading.Minute startOfTrading.Second date
      let endOfTradingDateTime = localDateToDateTime endOfTrading.Hour endOfTrading.Minute endOfTrading.Second date
      createMInterval [intervalBetween startOfTradingDateTime endOfTradingDateTime]
    )
  |> Option.getOrElse emptyMInterval

// returns an MInterval spanning the time of the holiday - this MInterval represents the time we take off for the holiday
let defaultHolidaySchedule (date: LocalDate): MInterval =
  if isAnyHoliday date then
    defaultTradingSchedule date
  else
    emptyMInterval


let buildTradingSchedule (normalTradingSchedule: TradingSchedule) (holidaySchedule: TradingSchedule) (date: LocalDate): MInterval =
  let tradingHours = normalTradingSchedule date
  let holidayHours = holidaySchedule date
  if overlaps holidayHours tradingHours then
    subtractMInterval tradingHours holidayHours
  else
    tradingHours

// returns true if the trading-schedule has any trading hours scheduled for that date; false otherwise.
let isTradingDay (tradingSchedule: TradingSchedule) (date: LocalDate): bool = not <| Seq.isEmpty (tradingSchedule date)

let tradingDays (tradingSchedule: TradingSchedule) (startDate: LocalDate): seq<LocalDate> =
  infPeriodicalDateSeries startDate (Period.FromDays(1L)) |> Seq.filter (isTradingDay tradingSchedule)

let tradingDaysBetween (tradingSchedule: TradingSchedule) (startDate: LocalDate) (endDate: LocalDate): seq<LocalDate> =
  tradingDays tradingSchedule startDate |> Seq.takeWhile (fun date -> date <= endDate)


let allTradingDaysCache = buildLruCache<TradingSchedule, TreeSet<LocalDate>> 2
let getFullTradingSchedule = get allTradingDaysCache
let putFullTradingSchedule = put allTradingDaysCache

// returns a TreeSet<LocalDate> of all the trading days in the timespan that we're interested in: 1950 - 2050.
let allTradingDays (tradingSchedule: TradingSchedule): TreeSet<LocalDate> =
  let cachedTradingDaysSet = getFullTradingSchedule tradingSchedule
  match cachedTradingDaysSet with
  | Some cachedTradingDaysSetElement -> cachedTradingDaysSetElement
  | None ->
      let newTradingDaysSet = new TreeSet<LocalDate>()
      newTradingDaysSet.AddAll(tradingDaysBetween tradingSchedule firstTradingDay lastTradingDay)
//      tradingDaysBetween firstTradingDay lastTradingDay tradingSchedule |> Seq.iter (fun tradingDay -> newTradingDaysSet.Add(tradingDay) |> ignore)
      putFullTradingSchedule tradingSchedule newTradingDaysSet
      newTradingDaysSet

let nextTradingDay (tradingSchedule: TradingSchedule) (date: LocalDate) (timeIncrement: Period): LocalDate =
  let tradingDaysTreeSet = allTradingDays tradingSchedule
  let nextDay = date + timeIncrement
  if tradingDaysTreeSet.Contains(nextDay) then nextDay
  else
    tradingDaysTreeSet.TrySuccessor(nextDay) 
    |> outParamToOpt 
    |> Option.getOrElseLazy (lazy (tradingDays tradingSchedule nextDay |> Seq.head))
