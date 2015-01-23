module Test.IntList

open System
open System.IO
open NUnit.Framework
open FsUnit

open dke.tradesim.IntList

[<Test>]
let ``bit writing and reading`` () = 
  let bw = new BitWriter()
  bw.write 6I 3
  bw.write 3I 2
  bw.write 1I 3
  bw.write 481I 9
  bw.write 67I 7
  bw.close()

  printfn "%A" (bw.toByteArray())

  let br = new BitReader(new MemoryStream(bw.toByteArray()))
  br.read 1 |> should equal 1
  br.read 3 |> should equal 5
  br.read 4 |> should equal 9
  br.read 4 |> should equal 15
  br.read 8 |> should equal 12
  br.read 4 |> should equal 3
  //intercept[IOException] {
  //  br.read(4)
  //}
