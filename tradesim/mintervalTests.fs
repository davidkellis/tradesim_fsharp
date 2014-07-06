module Test.MInterval

open NUnit.Framework
open FsUnit

open dke.tradesim.Time
open dke.tradesim.MInterval

[<Test>]
let ``emptyMInterval should return empty array`` () = 
  emptyMInterval |> should equal [| |]

[<Test>]
let ``createMInterval should return sorted array of non-overlapping intervals`` () = 
  let i1 = intervalBetween <| datetime 2000 01 01 0 0 0 <| datetime 2010 01 01 0 0 0
  let i2 = intervalBetween <| datetime 2012 01 01 0 0 0 <| datetime 2016 01 01 0 0 0
  createMInterval [i2; i1] |> should equal [| i1; i2 |]

[<Test>]
let ``subtractInterval should return an MInterval consisting of non-overlapping sub intervals`` () = 
  let i1 = intervalBetween <| datetime 2000 01 01 0 0 0 <| datetime 2010 01 01 0 0 0
  let i2 = intervalBetween <| datetime 2000 01 01 0 0 0 <| datetime 2005 01 01 0 0 0
  subtractInterval i1 i2 |> should equal [| intervalBetween <| datetime 2005 01 01 0 0 0 <| datetime 2010 01 01 0 0 0 |]

[<Test>]
let ``subtractMInterval subtracts one MInterval from another`` () = 
  let minuend = [|
    intervalBetween <| datetime 2013 1 1 8 0 0 <| datetime 2013 1 1 12 0 0;
    intervalBetween <| datetime 2013 1 1 12 0 0 <| datetime 2013 1 1 17 0 0
  |]
  let subtrahend = [|
    intervalBetween <| datetime 2013 1 1 10 0 0 <| datetime 2013 1 1 16 0 0
  |]
  let difference = [|
    intervalBetween <| datetime 2013 1 1 8 0 0 <| datetime 2013 1 1 10 0 0;
    intervalBetween <| datetime 2013 1 1 16 0 0 <| datetime 2013 1 1 17 0 0
  |]
  subtractMInterval minuend subtrahend |> should equal difference
