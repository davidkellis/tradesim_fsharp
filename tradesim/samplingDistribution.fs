module dke.tradesim.SamplingDistribution

open System
open System.Threading.Tasks

module Range =
  let iter (startI: int) (endI: int) (fn: int -> unit): unit =
    for i = startI to endI do
      fn i
  
  let map (startI: int) (endI: int) (fn: int -> 'T): array<'T> =
    let length = endI - startI + 1
    if length < 0 then
      failwith "The start of the range must not exceed the end of the range."
    else
      Array.init length (fun i -> fn (i + startI))
      
  let piter (startI: int) (endI: int) (fn: int -> unit): unit =
    Parallel.For(startI, endI, (fun i -> fn i)) |> ignore

  let pmap (startI: int) (endI: int) (fn: int -> 'T): array<'T> =
    let length = endI - startI + 1
    if length < 0 then
      failwith "The start of the range must not exceed the end of the range."
    else
      Array.Parallel.init length (fun i -> fn (i + startI))

module Decimal = 
  let Zero = 0M
  let One = 1M
  let Billionth = 0.000000001m

  let ceil (d: decimal): decimal = System.Math.Ceiling(d)

  let floor (d: decimal): decimal = System.Math.Floor(d)

  let isSignSame a b = (a < 0m) = (b < 0m)

  let isSignDiff a b = not (isSignSame a b)

  let rec pow (x: decimal) (exp: int): decimal =
    if exp > 0 then
      let mutable acc = 1m
      for i = 1 to exp do
        acc <- acc * x
      acc
    elif exp = 0 then
      1m
    else  // exp < 0
      1m / pow x -exp

  // This is a recursive implementation of the Bisection Method as defined by pseudocode in
  // Bisection3 on page 95 of Numerical Mathematics and Computing (5th ed.) by Ward/Kincaid.
  //
  // Arguments:
  // a, b define the interval within which the root is guaranteed to exist (i.e. a < root < b)
  // fa, fb are f(a) and f(b) respectively
  // n-max is the maximum iterations to perform
  // epsilon is an error threshold. The algorithm continues iterating until the error is less than epsilon.
  // n is the current iteration
  //
  // Returns:
  // [root-approximation error number-of-iterations]
  let rec findRootWithBisectionMethodR (f: decimal -> decimal) (a: decimal) (b: decimal) (fa: decimal) (fb: decimal) (maxN: int) (epsilon: decimal) (n: int): decimal =
    let error = (b - a) / 2m     // error <- (b - a) / 2
    let c = a + error            // c <- a + error  (c is the midpoint between a and b ; this is our best root approximation)
    let fc = (f c)               // fc <- f(c)      (fc is f evaluated at the midpoint between a and b)
    let n' = n + 1
    if ((abs error) < epsilon) || (n' > maxN) then  // our error is less than the error threshold epsilon OR we've executed the maximum number of iterations - in either case we have converged enough, so return c
      c
    else
      if isSignDiff fa fc then
        findRootWithBisectionMethodR f a c fa fc maxN epsilon n'
      else
        findRootWithBisectionMethodR f c b fc fb maxN epsilon n'

  let findRootWithBisectionMethod5 f a b maxN epsilon: decimal = findRootWithBisectionMethodR f a b (f a) (f b) maxN epsilon 1
  let findRootWithBisectionMethod f a b: decimal = findRootWithBisectionMethod5 f a b 300 Billionth

  // todo: implement Brent's root finding method: http://en.wikipedia.org/wiki/Brent%27s_method

  // Returns the nth root of A
  // -> A^(1/n)
  // It works by finding the positive root (i.e. positive zero) of the function f(x) = x^n - A
  // It returns the positive x at which f(x) = 0.
  //
  // Arguments:
  // n is the root we want to find (i.e. the "n" in "nth root")
  // A is the positive real number that we want to find the nth root of.
  //
  // Usage:
  //   (nth-root 45.13579 3)
  //   -> 3.5604674194663124
  //   Check the result:
  //   (expt 3.5604674194663124 3)
  //   45.13578999552352        (that's pretty close!)
  //
  //   (nth-root 0.456 4)
  //   -> 882350387/1073741824       ; (float (nth-root 0.456 4)) = 0.82175285
  //   Check the result:
  //   (float (expt 882350387/1073741824 4))
  //   -> 0.456
  let nthRoot (a: decimal) (n: int): decimal =
    let f = (fun x -> (pow x n) - a)
    if a < 1m then
      findRootWithBisectionMethod f 0m 1m
    else
      findRootWithBisectionMethod f 0m a

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


let sampleWithReplacement (n: int) (xs: array<'T>): array<'T> =
  let length = Array.length xs
  let rnd = new Random(int DateTime.Now.Ticks)
  Array.init n (fun i -> xs.[rnd.Next(0, length)])

let buildBootstrapSample (xs: array<'T>): array<'T> = sampleWithReplacement xs.Length xs
  
  
(*
  // returns array of sampling distributions s.t. the ith sampling distribution is the sampling distribution of the statistic computed by compute_sample_statistic_fns[i]
  let buildSamplingDistributions nSamples nObservationsPerSample buildSampleFn computeSampleStatisticFns =
    let samplingDistributions = Array.init (Array.length computeSampleStatisticFns) (fun _ -> Array.create nSamples 0m)

    for sampleIndex = 0 to (nSamples - 1) do
      let sample = buildSampleFn nObservationsPerSample
      computeSampleStatisticFns |> Seq.iteri (fun i computeSampleStatisticFn -> samplingDistributions.[i].[sampleIndex] <- computeSampleStatisticFn sample)

    samplingDistributions
*)

// returns array of sampling distributions s.t. the ith sampling distribution is the sampling distribution of the statistic computed by compute_sample_statistic_fns[i]
let buildSamplingDistributionsFromOneMultiStatisticFn
      nSamples 
      nObservationsPerSample 
      (buildSampleFn: int -> array<decimal>) 
      (multiStatisticFn: array<decimal> -> array<decimal>)
      : array<array<decimal>> =
  let diagnosticSample = [|1m; 2m; 3m|]
  let numberOfSamplingDistributions = multiStatisticFn diagnosticSample |> Array.length
  let samplingDistributions = Array.init numberOfSamplingDistributions (fun _ -> Array.create nSamples 0m)

  Range.piter 0 (nSamples - 1) (fun sampleIndex ->
    let sample = buildSampleFn nObservationsPerSample
    let sampleStatistics = multiStatisticFn sample
    sampleStatistics |> Array.iteri (fun i sampleStatistic -> samplingDistributions.[i].[sampleIndex] <- sampleStatistic)
  )
  
  samplingDistributions

let periodizeReturn (numberOfShortPeriodsUsedToCalculateValue: int) (numberOfShortPeriodsInLongPeriod: int) (returnValue: decimal): decimal =
  Decimal.pow (Decimal.nthRoot returnValue numberOfShortPeriodsUsedToCalculateValue) numberOfShortPeriodsInLongPeriod

let annualizeMonthlyReturn months (returnValue: decimal): decimal = periodizeReturn months 12 returnValue

let calculateCumulativeReturn (returns: array<decimal>): decimal = returns |> Array.reduce (fun acc d -> acc * d)

let annualizeMonthlyCumulativeReturn (returns: array<decimal>): decimal = calculateCumulativeReturn returns |> annualizeMonthlyReturn returns.Length

let buildSample (nObservations: int) (buildObservationFn: unit -> 'T): array<'T> = Array.init nObservations (fun i -> buildObservationFn () )

let buildCumulativeReturnSample (observedReturns: array<decimal>) numberOfTimePeriodsToConcatenate sampleSizeN: array<decimal> =
  let rnd = new Random(int DateTime.Now.Ticks)
  let observedReturnsLength = Array.length observedReturns
  
  let rec calculateCumulativeReturnR (rnd: Random) nObservationsToMultiply (observedReturns: array<decimal>) (observedReturnsLength: int) (acc: decimal) i: decimal =
    if i = nObservationsToMultiply then
      acc
    else
      let newAcc = acc * observedReturns.[rnd.Next(0, observedReturnsLength)]
      calculateCumulativeReturnR rnd nObservationsToMultiply observedReturns observedReturnsLength newAcc (i + 1)

  Range.map 1 sampleSizeN (fun _ -> calculateCumulativeReturnR rnd numberOfTimePeriodsToConcatenate observedReturns observedReturnsLength 1m 0)


(* let monthlyReturnsF = [| 23.2; 15.1; 2.4; -3.9; 25.1; 7.6; -2.8;  -12.6; -3.5; 5.6; -2.2; 11.8; 16.5; 30.9; 5.0; 35.9; -2.8; -25.5; 21.3;  9.4; 15.0; 22.5; -5.5; 18.1; -13.7; 20.3; -3.9; 10.5; -0.3; 0.2; -11.5;  31.6; -13.3; 14.4; 1.1; 12.9; 5.0; -16.8; -0.7; 1.3; 0.2; 18.9; 15.5; -11.7; 9.2; -11.8 |] *)
(* let monthlyReturnsF = [| 23.2; 15.1; 2.4; -3.9; 25.1; 7.6; -2.8;  -12.6; -3.5; 5.6; -2.2; 11.8; 16.5; 30.9; 5.0; 35.9; -2.8; -25.5; 21.3;  9.4; 15.0; 22.5; -5.5; 18.1; -13.7; 20.3; -3.9; 10.5; -0.3; 0.2; -11.5;  31.6; -13.3; 14.4; 1.1; 12.9; 5.0; -16.8; -0.7; 1.3; 0.2; 18.9; 15.5; -11.7; 9.2; -11.8; -25.6 |] *)
(* let monthlyReturnsF = [| 0.2; 18.9; 15.5; -11.7; 9.2; -11.8 |] *)
(* let monthlyReturnsF = [| -16.8; 0.2; 18.9; 15.5; -11.7; 9.2; -11.8 |] *)
let monthlyReturnsF = [| 0.2; 18.9; 15.5; -11.7; 9.2; -11.8; -17.2 |]
let monthlyReturns = monthlyReturnsF |> Array.map (fun r -> (decimal r + 100m) / 100m)

(* let build1YearReturnObservationFn () = sampleWithReplacement 12 monthlyReturns |> calculateCumulativeReturn *)
let build1YearReturnObservationFn () = buildBootstrapSample monthlyReturns |> annualizeMonthlyCumulativeReturn
let build5YearReturnObservationFn () = sampleWithReplacement 60 monthlyReturns |> calculateCumulativeReturn

// returns an array of returns
(* let build1YearReturnSample nObservations: array<decimal> = buildSample nObservations build1YearReturnObservationFn *)
let build1YearReturnSample nObservations: array<decimal> = buildCumulativeReturnSample monthlyReturns 12 nObservations
let build5YearReturnSample nObservations: array<decimal> = buildSample nObservations build5YearReturnObservationFn

let computeSampleMean sample = 
  let n = Array.length sample |> decimal
  Array.sum sample / n

let computeAllStats (sample: array<decimal>): array<decimal> = 
  Array.append
    [| computeSampleMean sample |]
    (percentiles [|1m; 5m; 10m; 20m; 30m; 40m; 50m; 60m; 70m; 80m; 90m; 95m; 99m|] sample)

(*
 * let t1 = DateTime.Now
 * 
 * let numberOfSamples = 10000
 * let numberOfObservationsPerSample = 10000
 * // samplingDistributions = build_sampling_distributions(number_of_samples, number_of_observations_per_sample, build_1_year_return_sample_fn, sample_statistic_fns)
 * let samplingDistributions = buildSamplingDistributionsFromOneMultiStatisticFn numberOfSamples numberOfObservationsPerSample build1YearReturnSample computeAllStats
 * 
 * let t2 = DateTime.Now
 * printfn "Building sampling distributions from %i samples, each containing %i observations, took %A seconds." numberOfSamples numberOfObservationsPerSample (t2 - t1)
 * 
 * let returnPercentiles = samplingDistributions |> Array.map (percentiles [| 1m; 10m; 20m; 30m; 40m; 50m; 60m; 70m; 80m; 90m; 99m |])
 * printfn "%A" returnPercentiles
 * 
 * let samplingDistributionMeans = samplingDistributions |> Array.map computeSampleMean
 * printfn "%A" samplingDistributionMeans
 *)