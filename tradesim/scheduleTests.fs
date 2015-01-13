module Test.Schedule

open NUnit.Framework
open FsUnit

open dke.tradesim.Time
open dke.tradesim.MInterval
open dke.tradesim.Schedule

[<Test>]
let ``returns a single-argument function that returns an MInterval representing the time interval on the given date that is considered to be 'in' the schedule`` () = 
  let tradingSchedule = buildTradingSchedule defaultNormalTradingSchedule defaultHolidaySchedule

  let regularBusinessHoursInterval = intervalBetween <| datetime 2013 3 28 9 30 0 <| datetime 2013 3 28 16 0 0
  tradingSchedule <| date 2013 3 28
  |> should equal <| createMInterval [regularBusinessHoursInterval]

  let holidayHours = emptyMInterval
  tradingSchedule <| date 2013 3 29 |> should equal holidayHours

[<Test>]
let ``isTradingDay returns true if the given LocalDate represents a trading day, according to the given trading schedule`` () = 
  let tradingSchedule = buildTradingSchedule defaultNormalTradingSchedule defaultHolidaySchedule
  isTradingDay tradingSchedule <| date 2013 3 29
  |> should equal false

[<Test>]
let ``tradingDays returns a sequence of LocalDates representing the trading schedule of a given time period`` () = 
  let tradingSchedule = buildTradingSchedule defaultNormalTradingSchedule defaultHolidaySchedule
  let days = tradingDays tradingSchedule <| date 2013 3 27 |> Seq.take 4
  Seq.toList days
  |> should equal [date 2013 3 27; date 2013 3 28; date 2013 4 1; date 2013 4 2]

[<Test>]
let ``nextTradingDay returns the next trading day in the given trading schedule`` () = 
  let tradingSchedule = buildTradingSchedule defaultNormalTradingSchedule defaultHolidaySchedule
  let startDate = date 2013 3 26
  let tradingDay1 = nextTradingDay tradingSchedule startDate (days 1L)
  let tradingDay2 = nextTradingDay tradingSchedule tradingDay1 (days 1L)
  let tradingDay3 = nextTradingDay tradingSchedule tradingDay2 (days 1L)
  let tradingDay4 = nextTradingDay tradingSchedule  tradingDay3 (days 1L)
  let tradingDays = [tradingDay1; tradingDay2; tradingDay3; tradingDay4]
  let expectedTradingDays = [date 2013 3 27; date 2013 3 28; date 2013 4 1; date 2013 4 2]    // 3/29/2013 is Good Friday
  tradingDays |> should equal expectedTradingDays
