﻿module dke.tradesim.Stats

open System.Collections.Generic

open FSharp.Collections
open FSharp.Collections.ParallelSeq

open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra
open MathNet.Numerics.LinearRegression

open Math
open dke.tradesim.Seq

let sampleWithReplacement = Array.getRandomElements
let sampleWithoutReplacement = Array.getRandomElements     // todo, implement this

// numberOfSamples is the number of samples to generate
// sampleSize is the number of elements that should be placed into each sample
// returns an array of samples, where each sample is an array of elements from the original array <xs>
let sample numberOfSamples sampleSize withReplacement (xs: array<'T>): array<array<'T>> =
  if not withReplacement && sampleSize > xs.Length then
    failwith "When sampling from a set of observations without replacement, the desired sample size cannot be greater than the number of observations"

  let samplesToBuild = [1 .. numberOfSamples] |> PSeq.withDegreeOfParallelism System.Environment.ProcessorCount   // mono uses ProcessorCount + 1 threads by default

  let mappingFn = 
    if withReplacement then
      (fun _ -> sampleWithReplacement sampleSize xs)
    else
      (fun _ -> sampleWithoutReplacement sampleSize xs)

  samplesToBuild
  |> PSeq.map mappingFn
  |> PSeq.toArray

let buildBootstrapSample (xs: array<'T>): array<'T> = sampleWithReplacement xs.Length xs

let buildBootstrapSamples numberOfSamples (originalSample: array<'T>): array<array<'T>> = 
  sample numberOfSamples (Array.length originalSample) true originalSample
  
let buildBootstrapDistribution (statisticFn: array<'T> -> 'U) numberOfSamples (originalSample: array<'T>): array<'U> =
  Array.Parallel.init numberOfSamples (fun i -> buildBootstrapSample originalSample |> statisticFn)

let buildBootstrapDistributions (statisticFn: array<'T> -> 'U) numberOfSamples (originalSample: array<'T>): array<'U> =
  Array.Parallel.init numberOfSamples (fun i -> buildBootstrapSample originalSample |> statisticFn)

module Sample =
  module Decimal =
    // copied from http://www.johndcook.com/standard_deviation.html
    //   except for the min/max logic
    type OnlineVariance() =
      let mutable k: int64 = 0L
      let mutable m_k: decimal = 0M
      let mutable s_k: decimal = 0M
      let mutable minValue: decimal = 0M
      let mutable maxValue: decimal = 0M

      member this.pushAll(xs: seq<decimal>) = Seq.iter this.push xs

      // invariant:
      // m_k = m_kMinus1 + (x_k - m_kMinus1) / k
      // s_k = s_kMinus1 + (x_k - m_kMinus1) * (x_k - m_k)
      member this.push(x: decimal) =
        if k = 0L then
          minValue <- x
          maxValue <- x
        else
          if x < minValue then
            minValue <- x
          elif x > maxValue then
            maxValue <- x

        k <- k + 1L

        // See Knuth TAOCP vol 2, 3rd edition, page 232
        if k = 1L then
          m_k <- x
          s_k <- 0M
        else
          let m_kPlus1 = m_k + (x - m_k) / decimal k
          let s_kPlus1 = s_k + (x - m_k) * (x - m_kPlus1)
          m_k <- m_kPlus1
          s_k <- s_kPlus1

      member this.n: int64 = k

      member this.mean: decimal = if k > 0L then m_k else 0M

      member this.variance: decimal = if k > 1L then s_k / decimal (k - 1L) else 0M

      member this.stdDev: decimal = Decimal.sqrt this.variance

      member this.min: Option<decimal> = if k > 0L then Some minValue else None

      member this.max: Option<decimal> = if k > 0L then Some maxValue else None


    type OnlineMeanMinMax() =
      let mutable k: int64 = 0L
      let mutable m_k: decimal = 0M
      let mutable minValue: decimal = 0M
      let mutable maxValue: decimal = 0M

      member this.pushAll(xs: seq<decimal>) = Seq.iter this.push xs

      member this.push(x: decimal) =
        if k = 0L then
          minValue <- x
          maxValue <- x
        else
          if x < minValue then
            minValue <- x
          elif x > maxValue then
            maxValue <- x

        k <- k + 1L

        // See Knuth TAOCP vol 2, 3rd edition, page 232
        if k = 1L then
          m_k <- x
        else
          let m_kPlus1 = m_k + (x - m_k) / decimal k
          m_k <- m_kPlus1


      member this.n: int64 = k

      member this.mean: decimal = if k > 0L then m_k else 0M

      member this.min: Option<decimal> = if k > 0L then Some minValue else None

      member this.max: Option<decimal> = if k > 0L then Some maxValue else None
        

    // copied from http://www.johndcook.com/running_regression.html
    type OnlineRegression() =
      let xStats = new OnlineVariance()
      let yStats = new OnlineVariance()
      let mutable S_xy = 0M
      let mutable n = 0L
      
      member this.push(x: decimal, y: decimal) =
        S_xy <- S_xy + (xStats.mean - x) * (yStats.mean - y) * decimal n / decimal (n + 1L)

        xStats.push(x)
        yStats.push(y)
        n <- n + 1L

      member this.slope: decimal =
        let S_xx = xStats.variance * decimal (n - 1L)
        S_xy / S_xx

      member this.intercept: decimal = yStats.mean - this.slope * xStats.mean

      member this.correlation: decimal =
        let t = xStats.stdDev * yStats.stdDev
        S_xy / (decimal (n - 1L) * t)


    // returns the sample correlation coefficient (Pearson's r coefficient)
    let correlation (xs: seq<decimal>) (ys: seq<decimal>): decimal =
      let pairs = Seq.zip xs ys
      let onlineCorrelation = new OnlineRegression()
      Seq.iter (fun (x, y) -> onlineCorrelation.push(x, y)) pairs
      onlineCorrelation.correlation


    let stdDev (xs: seq<decimal>): decimal =
      let onlineVariance = new OnlineVariance()
      Seq.iter onlineVariance.push xs
      onlineVariance.stdDev

    let variance (xs: seq<decimal>): decimal =
      let onlineVariance = new OnlineVariance()
      Seq.iter onlineVariance.push xs
      onlineVariance.variance

  //  let variance (xs: seq<decimal>): decimal = 
  //    // onlineVariance based on http://www.johndcook.com/standard_deviation.html
  //    let rec onlineVariance (enum: IEnumerator<decimal>) (m_k: decimal) (s_k: decimal) (k: int64): decimal =
  //      if enum.MoveNext() then
  //        let kPlus1 = k + 1L
  //        let x_kPlus1 = enum.Current
  //        let m_kPlus1 = m_k + (x_kPlus1 - m_k) / decimal kPlus1
  //        let s_kPlus1 = s_k + (x_kPlus1 - m_k) * (x_kPlus1 - m_kPlus1)
  //        onlineVariance enum m_kPlus1 s_kPlus1 kPlus1
  //      else
  //        if k > 1L then
  //          s_k / decimal (k - 1L)
  //        else
  //          0M
  //    
  //    if Seq.isEmpty xs then
  //      0M
  //    else
  //      let enum = xs.GetEnumerator()
  //      enum.MoveNext() |> ignore
  //      onlineVariance enum enum.Current 0M 1L
    

    type OlsResult = {slope: decimal; intercept: decimal}
    let simpleOls (xs: seq<decimal>) (ys: seq<decimal>): OlsResult =
      let pairs = Seq.zip xs ys
      let onlineRegression = new OnlineRegression()
      Seq.iter (fun (x, y) -> onlineRegression.push(x, y)) pairs
      {slope = onlineRegression.slope; intercept = onlineRegression.intercept}

    module Seq =
      let mean (xs: seq<decimal>): decimal =
        let rec onlineMean (enum: IEnumerator<decimal>) (sum: decimal) (length: int64): decimal =
          if enum.MoveNext() then
            onlineMean enum (sum + enum.Current) (length + 1L)
          else
            sum / decimal length
        onlineMean (xs.GetEnumerator()) 0M 0L

      // This implementation is based on http://en.wikipedia.org/wiki/Quantiles#Estimating_the_quantiles_of_a_population
      // For additional information, see:
      // http://www.stanford.edu/class/archive/anthsci/anthsci192/anthsci192.1064/handouts/calculating%20percentiles.pdf
      // http://en.wikipedia.org/wiki/Percentile
      // http://www.mathworks.com/help/stats/quantiles-and-percentiles.html
      // 
      // hFn is the function:
      //   (n: decimal) -> (p: decimal) -> decimal
      //   such that hFn returns a 1-based real-valued index (which may or may not be a whole-number) into the array of sorted values in xs
      // qSubPFn is the function:
      //   (getIthX: (int -> decimal)) -> (h: decimal) -> decimal
      //   such that getIthX returns the zero-based ith element from the array of sorted values in xs
      // percentages is a sequence of percentages expressed as real numbers in the range [0.0, 100.0]
      let quantiles
          (hFn: decimal -> decimal -> decimal) 
          (qSubPFn: (int -> decimal) -> decimal -> decimal) 
          (interpolate: bool) 
          (isSorted: bool) 
          (percentages: seq<decimal>)
          (xs: seq<decimal>)
          : seq<decimal> =
        let sortedXs = (if isSorted then xs else Seq.sort xs) |> Seq.toArray
        let n = decimal sortedXs.Length   // n is the sample size
        let q = 100m
        let p k = k / q
        let subtract1 = (fun x -> x - 1m)
        let hs = Seq.map (p >> hFn n >> subtract1) percentages   // NOTE: these indices are 0-based indices into sortedXs
        let getIthX = Array.get sortedXs

        if interpolate then                   // interpolate
          Seq.map
            (fun h ->
              let i = Decimal.floor h         // i is a 0-based index into sortedXs   (smaller index to interpolate between)
              let j = Decimal.ceil h          // j is a 0-based index into sortedXs   (larger index to interpolate between)
              let f = h - i                   // f is the fractional part of real-valued index h
              let intI = int i
              let intJ = int j
              (1m - f) * getIthX intI + f * getIthX intJ    // [1] - (1-f) * x_k + f * x_k+1 === x_k + f*(x_k+1 - x_k)
              // [1]:
              // see: http://web.stanford.edu/class/archive/anthsci/anthsci192/anthsci192.1064/handouts/calculating%20percentiles.pdf
              // also: (1-f) * x_k + f * x_k+1 === x_k - f*x_k + f*x_k+1 === x_k + f*(x_k+1 - x_k) which is what I'm after
            )
            hs
        else                                  // floor the index instead of interpolating
          Seq.map
            (fun h ->
              let i = int (Decimal.floor h)   // i is a 0-based index into sortedXs
              getIthX i
            )
            hs
      
      // implementation based on description of R-1 at http://en.wikipedia.org/wiki/Quantile#Estimating_the_quantiles_of_a_population
      let quantilesR1 (interpolate: bool) (isSorted: bool) (percentages: seq<decimal>) (xs: seq<decimal>): seq<decimal> =
        quantiles 
          (fun (n: decimal) (p: decimal) -> if p = 0m then 1m else n * p + 0.5m)
          (fun (getIthX: (int -> decimal)) (h: decimal) -> getIthX (int (Decimal.ceil (h - 0.5m))))
          interpolate
          isSorted
          percentages
          xs

      // The R manual claims that "Hyndman and Fan (1996) ... recommended type 8"
      // see: http://stat.ethz.ch/R-manual/R-patched/library/stats/html/quantile.html
      // implementation based on description of R-8 at http://en.wikipedia.org/wiki/Quantile#Estimating_the_quantiles_of_a_population
      let OneThird = 1m / 3m
      let TwoThirds = 2m / 3m
      let quantilesR8 (interpolate: bool) (isSorted: bool) (percentages: seq<decimal>) (xs: seq<decimal>): seq<decimal> =
        quantiles 
          (fun (n: decimal) (p: decimal) ->
            if p < TwoThirds / (n + OneThird) then 1m
            elif p >= (n - OneThird) / (n + OneThird) then n
            else (n + OneThird) * p + OneThird
          )
          (fun (getIthX: (int -> decimal)) (h: decimal) -> 
            let floorHDec = Decimal.floor h
            let floorH = int floorHDec
            getIthX floorH + (h - floorHDec) * (getIthX (floorH + 1) - getIthX floorH)
          )
          interpolate
          isSorted
          percentages
          xs

      // we use the type 8 quantile method because the R manual claims that "Hyndman and Fan (1996) ... recommended type 8"
      let percentiles percentages xs = quantilesR8 true false percentages xs
      let percentilesSorted percentages xs = quantilesR8 true true percentages xs

    module Array =
      let mean sample = 
        let n = Array.length sample |> decimal
        Array.sum sample / n

      let quantiles
          (hFn: decimal -> decimal -> decimal) 
          (qSubPFn: (int -> decimal) -> decimal -> decimal) 
          (interpolate: bool) 
          (isSorted: bool) 
          (percentages: array<decimal>)
          (xs: array<decimal>)
          : array<decimal> =
        let sortedXs = (if isSorted then xs else Array.sort xs)
        let n = decimal sortedXs.Length   // n is the sample size
        let q = 100m
        let p k = k / q
        let subtract1 = (fun x -> x - 1m)
        let hs = percentages |> Array.map (p >> hFn n >> subtract1)   // NOTE: these indices are 0-based indices into sortedXs
        let getIthX = Array.get sortedXs

        if interpolate then                   // interpolate
          Array.map
            (fun h ->
              let i = Decimal.floor h         // i is a 0-based index into sortedXs   (smaller index to interpolate between)
              let j = Decimal.ceil h          // j is a 0-based index into sortedXs   (larger index to interpolate between)
              let f = h - i                   // f is the fractional part of real-valued index h
              let intI = int i
              let intJ = int j
              (1m - f) * getIthX intI + f * getIthX intJ    // [1] - (1-f) * x_k + f * x_k+1 === x_k + f*(x_k+1 - x_k)
              // [1]:
              // see: http://web.stanford.edu/class/archive/anthsci/anthsci192/anthsci192.1064/handouts/calculating%20percentiles.pdf
              // also: (1-f) * x_k + f * x_k+1 === x_k - f*x_k + f*x_k+1 === x_k + f*(x_k+1 - x_k) which is what I'm after
            )
            hs
        else                                  // floor the index instead of interpolating
          Array.map
            (fun h ->
              let i = int (Decimal.floor h)   // i is a 0-based index into sortedXs
              getIthX i
            )
            hs

      // implementation based on description of R-1 at http://en.wikipedia.org/wiki/Quantile#Estimating_the_quantiles_of_a_population
      let quantilesR1 (interpolate: bool) (isSorted: bool) (percentages: array<decimal>) (xs: array<decimal>): array<decimal> =
        quantiles 
          (fun (n: decimal) (p: decimal) -> if p = 0m then 1m else n * p + 0.5m)
          (fun (getIthX: (int -> decimal)) (h: decimal) -> getIthX (int (Decimal.ceil (h - 0.5m))))
          interpolate
          isSorted
          percentages
          xs

      // The R manual claims that "Hyndman and Fan (1996) ... recommended type 8"
      // see: http://stat.ethz.ch/R-manual/R-patched/library/stats/html/quantile.html
      // implementation based on description of R-8 at http://en.wikipedia.org/wiki/Quantile#Estimating_the_quantiles_of_a_population
      let OneThird = 1m / 3m
      let TwoThirds = 2m / 3m
      let quantilesR8 (interpolate: bool) (isSorted: bool) (percentages: array<decimal>) (xs: array<decimal>): array<decimal> =
        quantiles 
          (fun (n: decimal) (p: decimal) ->
            if p < TwoThirds / (n + OneThird) then 1m
            elif p >= (n - OneThird) / (n + OneThird) then n
            else (n + OneThird) * p + OneThird
          )
          (fun (getIthX: (int -> decimal)) (h: decimal) -> 
            let floorHDec = Decimal.floor h
            let floorH = int floorHDec
            getIthX floorH + (h - floorHDec) * (getIthX (floorH + 1) - getIthX floorH)
          )
          interpolate
          isSorted
          percentages
          xs

      // we use the type 8 quantile method because the R manual claims that "Hyndman and Fan (1996) ... recommended type 8"
      let percentiles percentages xs = quantilesR8 true false percentages xs
      let percentilesSorted percentages xs = quantilesR8 true true percentages xs


  // ***********************************************************************************************************
  // ********************************************* Double routines *********************************************
  // ***********************************************************************************************************

  module Double =
    (*
     * Example:
     *   ols(DenseMatrix((1.59, 1.0), (2.89, 1.0), (3.76, 1.0), (4.93, 1.0)), DenseVector(1.14, 2.54, 3.89, 4.18))
     *   -> breeze.linalg.DenseVector<Double> = DenseVector(0.9563205952545724, -0.21118555987567958)
     * Verification in R:
     *   > xs <- c(1.59, 2.89, 3.76, 4.93)
     *   > ys <- c(1.14, 2.54, 3.89, 4.18)
     *   > xs_m=matrix(c(xs, 1, 1, 1, 1), 4)
     *   > beta <- solve(t(xs_m) %*% xs_m) %*% t(xs_m) %*% ys
     *   > beta
     *              [,1]
     *   [1,]  0.9563206
     *   [2,] -0.2111856
     *)
    let ols (observations: Matrix<double>) (outputs: Vector<double>): Vector<double> =
      MultipleRegression.NormalEquations(observations, outputs)

    // function to create linear regression model based on ordinary least squares method of determining the slope and y-intercept of the linear model
    let linearModel (observations: Matrix<double>) (outputs: Vector<double>): seq<double> -> double =
      let beta = ols observations outputs |> Vector.toArray
      (fun (observation: seq<double>) ->
        Seq.zipWithIndex observation
        |> Seq.fold
             (fun sum (observedValue, index) ->
               let coefficient = beta.[index]
               sum + coefficient * observedValue
             )
             0.0
      )

    // copied from http://www.johndcook.com/standard_deviation.html
    //   except for the min/max logic
    type OnlineVariance() =
      let mutable k: int64 = 0L
      let mutable m_k: double = 0.0
      let mutable s_k: double = 0.0
      let mutable minValue: double = 0.0
      let mutable maxValue: double = 0.0

      member this.pushAll(xs: seq<double>) = Seq.iter this.push xs

      // invariant:
      // m_k = m_kMinus1 + (x_k - m_kMinus1) / k
      // s_k = s_kMinus1 + (x_k - m_kMinus1) * (x_k - m_k)
      member this.push(x: double) =
        if k = 0L then
          minValue <- x
          maxValue <- x
        else
          if x < minValue then
            minValue <- x
          elif x > maxValue then
            maxValue <- x

        k <- k + 1L

        // See Knuth TAOCP vol 2, 3rd edition, page 232
        if k = 1L then
          m_k <- x
          s_k <- 0.0
        else
          let m_kPlus1 = m_k + (x - m_k) / double k
          let s_kPlus1 = s_k + (x - m_k) * (x - m_kPlus1)
          m_k <- m_kPlus1
          s_k <- s_kPlus1

      member this.n: int64 = k

      member this.mean: double = if k > 0L then m_k else 0.0

      member this.variance: double = if k > 1L then s_k / double (k - 1L) else 0.0

      member this.stdDev: double = Double.sqrt this.variance

      member this.min: Option<double> = if k > 0L then Some minValue else None

      member this.max: Option<double> = if k > 0L then Some maxValue else None


    type OnlineMeanMinMax() =
      let mutable k: int64 = 0L
      let mutable m_k: double = 0.0
      let mutable minValue: double = 0.0
      let mutable maxValue: double = 0.0

      member this.pushAll(xs: seq<double>) = Seq.iter this.push xs

      member this.push(x: double) =
        if k = 0L then
          minValue <- x
          maxValue <- x
        else
          if x < minValue then
            minValue <- x
          elif x > maxValue then
            maxValue <- x

        k <- k + 1L

        // See Knuth TAOCP vol 2, 3rd edition, page 232
        if k = 1L then
          m_k <- x
        else
          let m_kPlus1 = m_k + (x - m_k) / double k
          m_k <- m_kPlus1


      member this.n: int64 = k

      member this.mean: double = if k > 0L then m_k else 0.0

      member this.min: Option<double> = if k > 0L then Some minValue else None

      member this.max: Option<double> = if k > 0L then Some maxValue else None
        

    // copied from http://www.johndcook.com/running_regression.html
    type OnlineRegression() =
      let xStats = new OnlineVariance()
      let yStats = new OnlineVariance()
      let mutable S_xy = 0.0
      let mutable n = 0L
      
      member this.push(x: double, y: double) =
        S_xy <- S_xy + (xStats.mean - x) * (yStats.mean - y) * double n / double (n + 1L)

        xStats.push(x)
        yStats.push(y)
        n <- n + 1L

      member this.slope: double =
        let S_xx = xStats.variance * double (n - 1L)
        S_xy / S_xx

      member this.intercept: double = yStats.mean - this.slope * xStats.mean

      member this.correlation: double =
        let t = xStats.stdDev * yStats.stdDev
        S_xy / (double (n - 1L) * t)


    // returns the sample correlation coefficient (Pearson's r coefficient)
    let correlation (xs: seq<double>) (ys: seq<double>): double =
      let pairs = Seq.zip xs ys
      let onlineCorrelation = new OnlineRegression()
      Seq.iter (fun (x, y) -> onlineCorrelation.push(x, y)) pairs
      onlineCorrelation.correlation


    let stdDev (xs: seq<double>): double =
      let onlineVariance = new OnlineVariance()
      Seq.iter onlineVariance.push xs
      onlineVariance.stdDev

    let variance (xs: seq<double>): double =
      let onlineVariance = new OnlineVariance()
      Seq.iter onlineVariance.push xs
      onlineVariance.variance

  //  let variance (xs: seq<double>): double = 
  //    // onlineVariance based on http://www.johndcook.com/standard_deviation.html
  //    let rec onlineVariance (enum: IEnumerator<double>) (m_k: double) (s_k: double) (k: int64): double =
  //      if enum.MoveNext() then
  //        let kPlus1 = k + 1L
  //        let x_kPlus1 = enum.Current
  //        let m_kPlus1 = m_k + (x_kPlus1 - m_k) / double kPlus1
  //        let s_kPlus1 = s_k + (x_kPlus1 - m_k) * (x_kPlus1 - m_kPlus1)
  //        onlineVariance enum m_kPlus1 s_kPlus1 kPlus1
  //      else
  //        if k > 1L then
  //          s_k / double (k - 1L)
  //        else
  //          0M
  //    
  //    if Seq.isEmpty xs then
  //      0M
  //    else
  //      let enum = xs.GetEnumerator()
  //      enum.MoveNext() |> ignore
  //      onlineVariance enum enum.Current 0M 1L
    

    type OlsResult = {slope: double; intercept: double}
    let simpleOls (xs: seq<double>) (ys: seq<double>): OlsResult =
      let pairs = Seq.zip xs ys
      let onlineRegression = new OnlineRegression()
      Seq.iter (fun (x, y) -> onlineRegression.push(x, y)) pairs
      {slope = onlineRegression.slope; intercept = onlineRegression.intercept}

    module Seq =
      let mean (xs: seq<double>): double =
        let rec onlineMean (enum: IEnumerator<double>) (sum: double) (length: int64): double =
          if enum.MoveNext() then
            onlineMean enum (sum + enum.Current) (length + 1L)
          else
            sum / double length
        onlineMean (xs.GetEnumerator()) 0.0 0L

      // This implementation is based on http://en.wikipedia.org/wiki/Quantiles#Estimating_the_quantiles_of_a_population
      // For additional information, see:
      // http://www.stanford.edu/class/archive/anthsci/anthsci192/anthsci192.1064/handouts/calculating%20percentiles.pdf
      // http://en.wikipedia.org/wiki/Percentile
      // http://www.mathworks.com/help/stats/quantiles-and-percentiles.html
      // 
      // hFn is the function:
      //   (n: double) -> (p: double) -> double
      //   such that hFn returns a 1-based real-valued index (which may or may not be a whole-number) into the array of sorted values in xs
      // qSubPFn is the function:
      //   (getIthX: (int -> double)) -> (h: double) -> double
      //   such that getIthX returns the zero-based ith element from the array of sorted values in xs
      // percentages is a sequence of percentages expressed as real numbers in the range [0.0, 100.0]
      let quantiles
          (hFn: double -> double -> double) 
          (qSubPFn: (int -> double) -> double -> double) 
          (interpolate: bool) 
          (isSorted: bool) 
          (percentages: seq<double>)
          (xs: seq<double>)
          : seq<double> =
        let sortedXs = (if isSorted then xs else Seq.sort xs) |> Seq.toArray
        let n = double sortedXs.Length   // n is the sample size
        let q = 100.0
        let p k = k / q
        let subtract1 = (fun x -> x - 1.0)
        let hs = Seq.map (p >> hFn n >> subtract1) percentages   // NOTE: these indices are 0-based indices into sortedXs
        let getIthX = Array.get sortedXs

        if interpolate then                   // interpolate
          Seq.map
            (fun h ->
              let i = Double.floor h         // i is a 0-based index into sortedXs   (smaller index to interpolate between)
              let j = Double.ceil h          // j is a 0-based index into sortedXs   (larger index to interpolate between)
              let f = h - i                   // f is the fractional part of real-valued index h
              let intI = int i
              let intJ = int j
              (1.0 - f) * getIthX intI + f * getIthX intJ    // [1] - (1-f) * x_k + f * x_k+1 === x_k + f*(x_k+1 - x_k)
              // [1]:
              // see: http://web.stanford.edu/class/archive/anthsci/anthsci192/anthsci192.1064/handouts/calculating%20percentiles.pdf
              // also: (1-f) * x_k + f * x_k+1 === x_k - f*x_k + f*x_k+1 === x_k + f*(x_k+1 - x_k) which is what I'm after
            )
            hs
        else                                  // floor the index instead of interpolating
          Seq.map
            (fun h ->
              let i = int (Double.floor h)   // i is a 0-based index into sortedXs
              getIthX i
            )
            hs
      
      // implementation based on description of R-1 at http://en.wikipedia.org/wiki/Quantile#Estimating_the_quantiles_of_a_population
      let quantilesR1 (interpolate: bool) (isSorted: bool) (percentages: seq<double>) (xs: seq<double>): seq<double> =
        quantiles 
          (fun (n: double) (p: double) -> if p = 0.0 then 1.0 else n * p + 0.5)
          (fun (getIthX: (int -> double)) (h: double) -> getIthX (int (Double.ceil (h - 0.5))))
          interpolate
          isSorted
          percentages
          xs

      // The R manual claims that "Hyndman and Fan (1996) ... recommended type 8"
      // see: http://stat.ethz.ch/R-manual/R-patched/library/stats/html/quantile.html
      // implementation based on description of R-8 at http://en.wikipedia.org/wiki/Quantile#Estimating_the_quantiles_of_a_population
      let OneThird = 1.0 / 3.0
      let TwoThirds = 2.0 / 3.0
      let quantilesR8 (interpolate: bool) (isSorted: bool) (percentages: seq<double>) (xs: seq<double>): seq<double> =
        quantiles 
          (fun (n: double) (p: double) ->
            if p < TwoThirds / (n + OneThird) then 1.0
            elif p >= (n - OneThird) / (n + OneThird) then n
            else (n + OneThird) * p + OneThird
          )
          (fun (getIthX: (int -> double)) (h: double) -> 
            let floorHDec = Double.floor h
            let floorH = int floorHDec
            getIthX floorH + (h - floorHDec) * (getIthX (floorH + 1) - getIthX floorH)
          )
          interpolate
          isSorted
          percentages
          xs

      // we use the type 8 quantile method because the R manual claims that "Hyndman and Fan (1996) ... recommended type 8"
      let percentiles percentages xs = quantilesR8 true false percentages xs
      let percentilesSorted percentages xs = quantilesR8 true true percentages xs

    module Array =
      let mean sample = 
        let n = Array.length sample |> double
        Array.sum sample / n

      let quantiles
          (hFn: double -> double -> double) 
          (qSubPFn: (int -> double) -> double -> double) 
          (interpolate: bool) 
          (isSorted: bool) 
          (percentages: array<double>)
          (xs: array<double>)
          : array<double> =
        let sortedXs = (if isSorted then xs else Array.sort xs)
        let n = double sortedXs.Length   // n is the sample size
        let q = 100.0
        let p k = k / q
        let subtract1 = (fun x -> x - 1.0)
        let hs = percentages |> Array.map (p >> hFn n >> subtract1)   // NOTE: these indices are 0-based indices into sortedXs
        let getIthX = Array.get sortedXs

        if interpolate then                   // interpolate
          Array.map
            (fun h ->
              let i = Double.floor h         // i is a 0-based index into sortedXs   (smaller index to interpolate between)
              let j = Double.ceil h          // j is a 0-based index into sortedXs   (larger index to interpolate between)
              let f = h - i                   // f is the fractional part of real-valued index h
              let intI = int i
              let intJ = int j
              (1.0 - f) * getIthX intI + f * getIthX intJ    // [1] - (1-f) * x_k + f * x_k+1 === x_k + f*(x_k+1 - x_k)
              // [1]:
              // see: http://web.stanford.edu/class/archive/anthsci/anthsci192/anthsci192.1064/handouts/calculating%20percentiles.pdf
              // also: (1-f) * x_k + f * x_k+1 === x_k - f*x_k + f*x_k+1 === x_k + f*(x_k+1 - x_k) which is what I'm after
            )
            hs
        else                                  // floor the index instead of interpolating
          Array.map
            (fun h ->
              let i = int (Double.floor h)   // i is a 0-based index into sortedXs
              getIthX i
            )
            hs

      // implementation based on description of R-1 at http://en.wikipedia.org/wiki/Quantile#Estimating_the_quantiles_of_a_population
      let quantilesR1 (interpolate: bool) (isSorted: bool) (percentages: array<double>) (xs: array<double>): array<double> =
        quantiles 
          (fun (n: double) (p: double) -> if p = 0.0 then 1.0 else n * p + 0.5)
          (fun (getIthX: (int -> double)) (h: double) -> getIthX (int (Double.ceil (h - 0.5))))
          interpolate
          isSorted
          percentages
          xs

      // The R manual claims that "Hyndman and Fan (1996) ... recommended type 8"
      // see: http://stat.ethz.ch/R-manual/R-patched/library/stats/html/quantile.html
      // implementation based on description of R-8 at http://en.wikipedia.org/wiki/Quantile#Estimating_the_quantiles_of_a_population
      let OneThird = 1.0 / 3.0
      let TwoThirds = 2.0 / 3.0
      let quantilesR8 (interpolate: bool) (isSorted: bool) (percentages: array<double>) (xs: array<double>): array<double> =
        quantiles 
          (fun (n: double) (p: double) ->
            if p < TwoThirds / (n + OneThird) then 1.0
            elif p >= (n - OneThird) / (n + OneThird) then n
            else (n + OneThird) * p + OneThird
          )
          (fun (getIthX: (int -> double)) (h: double) -> 
            let floorHDec = Double.floor h
            let floorH = int floorHDec
            getIthX floorH + (h - floorHDec) * (getIthX (floorH + 1) - getIthX floorH)
          )
          interpolate
          isSorted
          percentages
          xs

      // we use the type 8 quantile method because the R manual claims that "Hyndman and Fan (1996) ... recommended type 8"
      let percentiles percentages xs = quantilesR8 true false percentages xs
      let percentilesSorted percentages xs = quantilesR8 true true percentages xs