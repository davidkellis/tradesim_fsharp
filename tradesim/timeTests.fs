module Test.Time

open NUnit.Framework
open FsUnit

open dke.tradesim.Time
open dke.tradesim.SpecialDates

[<Test>]
let ``timestamp returns a timestamp representation, yyyymmddhhmmss, of a given DateTime`` () = 
  dateTimeToTimestamp <| datetime 2013 7 15 20 1 45
  |> should equal 20130715200145L

[<Test>]
let ``periodicalTimeSeries returns a sequence of DateTimes, [t1, t2, ..., tN], that are separated by a given Period, s.t. startTime = t1 and tN <= endTime`` () = 
  let startTime = datetime 2013 7 1 12 0 0
  let endTime = datetime 2013 7 2 12 0 0

  let timeSeries = periodicalTimeSeries startTime endTime (hours 6L) |> Seq.toList
  let expectedTimeSeries = [
    datetime 2013 7 1 12 0 0
    datetime 2013 7 1 18 0 0
    datetime 2013 7 2 0 0 0
    datetime 2013 7 2 6 0 0
    datetime 2013 7 2 12 0 0
  ]
  timeSeries |> should equal expectedTimeSeries

  let timeSeries2 = periodicalTimeSeries startTime endTime (hours 7L) |> Seq.toList
  let expectedTimeSeries2 = [
    datetime 2013 7 1 12 0 0
    datetime 2013 7 1 19 0 0
    datetime 2013 7 2 2 0 0
    datetime 2013 7 2 9 0 0
  ]
  timeSeries2 |> should equal expectedTimeSeries2


[<Test>]
let ``infInterspersedIntervals returns a sequence of Intervals, [i1, i2, ..., iN], s.t. the start time of subsequent intervals is separated by a given Period, <separationLength> and each interval spans an amount of time given by <intervalLength>`` () = 
  let startTime = datetime 2013 7 1 12 0 0
  let endTime = datetime 2013 7 2 12 0 0

  let intervals = infInterspersedIntervals startTime (hours 5L) (days 1L) |> Seq.take 5 |> Seq.toList
  let expectedIntervals = [
    intervalBetween <| datetime 2013 7 1 12 0 0 <| datetime 2013 7 1 17 0 0
    intervalBetween <| datetime 2013 7 2 12 0 0 <| datetime 2013 7 2 17 0 0
    intervalBetween <| datetime 2013 7 3 12 0 0 <| datetime 2013 7 3 17 0 0
    intervalBetween <| datetime 2013 7 4 12 0 0 <| datetime 2013 7 4 17 0 0
    intervalBetween <| datetime 2013 7 5 12 0 0 <| datetime 2013 7 5 17 0 0
  ]
  intervals |> should equal expectedIntervals


[<Test>]
let ``isAnyHoliday returns true if the given date is a holiday`` () = 
  isAnyHoliday <| date 2013 3 29 |> should equal true    // Good Friday


[<Test>]
let ``easter returns the DateTime that represents the beginning of the day on Easter, given a year`` () = 
  easter 2009 |> should equal <| date 2009 4 12
  easter 2010 |> should equal <| date 2010 4 4
  easter 2011 |> should equal <| date 2011 4 24
  easter 2012 |> should equal <| date 2012 4 8
  easter 2013 |> should equal <| date 2013 3 31


[<Test>]
let ``goodFriday returns the DateTime that represents the beginning of the day of Good Friday, given a year`` () = 
  goodFriday 2009 |> should equal <| date 2009 4 10
  goodFriday 2010 |> should equal <| date 2010 4 2
  goodFriday 2011 |> should equal <| date 2011 4 22
  goodFriday 2012 |> should equal <| date 2012 4 6
  goodFriday 2013 |> should equal <| date 2013 3 29
