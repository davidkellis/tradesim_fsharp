module Test.Stats

open System
open NUnit.Framework
open FsUnit
open MathNet.Numerics.LinearAlgebra

open dke.tradesim.Math
open dke.tradesim.Stats

[<Test>]
let ``correlation returns the sample correlation coefficient (Pearson's r coefficient)`` () = 
  // checked against wolfram alpha with this:
  //   N[Correlation[{1.59, 2.89, 3.76, 4.93, 5.0, 6.36, 7.35, 8.77, 9.19, 10.22}, {1.14, 2.54, 3.89, 4.18, 5.25, 6.3, 7.98, 8.54, 9.82, 10.41}], 20]
  let mutable xs = [1.59m; 2.89m; 3.76m; 4.93m; 5.0m; 6.36m; 7.35m; 8.77m; 9.19m; 10.22m]
  let mutable ys = [1.14m; 2.54m; 3.89m; 4.18m; 5.25m; 6.3m; 7.98m; 8.54m; 9.82m; 10.41m]
  Sample.correlation xs ys |> Decimal.roundTo 10 |> should equal 0.9925662059M

  // the following 3 examples taken from: http://www.biddle.com/documents/bcg_comp_chapter4.pdf
  xs <- [12m; 14m; 15m; 16m; 18m]
  ys <- [350000m; 399765m; 429000m; 435000m; 433000m]
  Sample.correlation xs ys |> Decimal.roundTo 3 |> should equal 0.880M

  xs <- [12m; 14m; 15m; 16m; 18m]
  ys <- [32m; 35m; 45m; 50m; 65m]
  Sample.correlation xs ys |> Decimal.roundTo 3 |> should equal 0.968M

  xs <- [350000m; 399765m; 429000m; 435000m; 433000m]
  ys <- [32m; 35m; 45m; 50m; 65m]
  Sample.correlation xs ys |> Decimal.roundTo 3 |> should equal 0.772M

[<Test>]
let ``mean returns sample mean`` () =
  let mutable xs = [12m; 14m; 15m; 16m; 18m]
  Sample.Seq.mean xs |> should equal 15m

  xs <- [32m; 35m; 45m; 50m; 65m]
  Sample.Seq.mean xs |> should equal 45.4m

  xs <- [350000m; 399765m; 429000m; 435000m; 433000m]
  Sample.Seq.mean xs |> should equal 409353m

[<Test>]
let ``stdDev computes sample standard deviation`` () =
  let mutable xs = [12m; 14m; 15m; 16m; 18m]
  Sample.stdDev xs |> Decimal.roundTo 3 |> should equal 2.236m

  xs <- [32m; 35m; 45m; 50m; 65m]
  Sample.stdDev xs |> Decimal.roundTo 3 |> should equal 13.164m

  xs <- [350000m; 399765m; 429000m; 435000m; 433000m]
  Sample.stdDev xs |> Decimal.roundTo 3 |> should equal 36116.693m

[<Test>]
// checked against wolfram alpha with this:
//   linear fit {1.59,1.14}, {2.89,2.54}, {3.76,3.89}, {4.93,4.18}
let ``simpleOls performs a linear regression`` () =
  let xs = [1.59m; 2.89m; 3.76m; 4.93m]
  let ys = [1.14m; 2.54m; 3.89m; 4.18m]
  let result = Sample.simpleOls xs ys
  result.slope |> Decimal.roundTo 6 |> should equal 0.956321m
  result.intercept |> Decimal.roundTo 6 |> should equal -0.211186m

[<Test>]
// checked against wolfram alpha with this:
//   linear fit {1.59,1.14}, {2.89,2.54}, {3.76,3.89}, {4.93,4.18}
let ``ols performs a linear regression`` () =
  let mutable xs: Matrix<double> = matrix [[1.59; 1.0]
                                           [2.89; 1.0]
                                           [3.76; 1.0]
                                           [4.93; 1.0]]
  let mutable ys: Vector<double> = vector [1.14; 2.54; 3.89; 4.18]
  let result = Sample.ols xs ys
  result.[0] |> Double.roundTo 10 |> should equal 0.9563205953
  result.[1] |> Double.roundTo 10 |> should equal -0.2111855599

[<Test>]
let ``linearModel estimates a linear equation that fits the given data points`` () =
  let f x1 x2 = (2.0 / 3.0) * x1 + 43.0 * x2 + 57.0

  let mutable xs: Matrix<double> =  matrix [[152.15; 8.21   ; 1.0]
                                            [346.6 ; -281.48; 1.0]
                                            [98.2  ; 198.2  ; 1.0]
                                            [-18.5 ; 17.686 ; 1.0]]
  let mutable ys: Vector<double> = vector [f 152.15 8.21    
                                           f 346.6  -281.48 
                                           f 98.2   198.2   
                                           f -18.5  17.686  ]
  let f' = Sample.linearModel xs ys

  f' [18.1; -992.2; 1.0] |> Double.roundTo 10 |> should equal (f 18.1 -992.2 |> Double.roundTo 10)
  f' [-1818.11; 0.418; 1.0] |> Double.roundTo 10 |> should equal (f -1818.11 0.418 |> Double.roundTo 10)

[<Test>]
// test example taken from http://web.stanford.edu/class/archive/anthsci/anthsci192/anthsci192.1064/handouts/calculating%20percentiles.pdf
let ``quantilesR8 should calculate the nth quantile`` () =
  let xs = [5; 1; 9; 3; 14; 9; 7] |> List.map decimal

  // in R:
  // x <- c(5, 1, 9, 3, 14, 9, 7)
  // quantile(x, probs = c(0, 0.1, 0.2, 0.25, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 1), na.rm = FALSE, names = TRUE, type = 8)
  Sample.Seq.percentiles [0m; 10m; 20m; 25m; 30m; 40m; 50m; 60m; 70m; 80m; 90m; 100m] xs 
  |> Seq.map (Decimal.roundTo 4) 
  |> should equal [1m; 1.1333m; 2.6000m; 3.3333m; 4.0667m; 5.5333m; 7m; 8.4667m; 9m; 10m; 13.6667m; 14m;]