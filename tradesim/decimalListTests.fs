module Test.DecimalList

open System
open System.IO
open System.Numerics
open NUnit.Framework
open FsUnit

open dke.tradesim
open Math

[<Test>]
let ``DecimalList encodes and decodes decimals`` () =
  let decimals = Random.randomIntsBetween -10000000 10000001 1000 |> Array.map (fun i -> decimal i / 100000M)
  let sortedTruncatedDecimals = Array.sort decimals |> Array.map (Decimal.roundTo 3)

  let bytes = DecimalList.encode 3 decimals

  //DecimalList.decode 3 bytes |> should equal sortedTruncatedDecimals

  Assert.AreEqual(
    DecimalList.decode 3 bytes,
    sortedTruncatedDecimals
  )
