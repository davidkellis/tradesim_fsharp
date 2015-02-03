module dke

open System
open System.Numerics
open System.Threading.Tasks

type Range = int * int

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module Range =
  let iter (fn: int -> unit) (range: Range): unit =
    let (startI, endI) = range
    for i = startI to endI do
      fn i

  // fold over the range [start, end)  (i.e. excluding the end)
  let fold (accFn: 'State -> int -> 'State) (state: 'State) (range: Range) =
    let rec foldR state i endI =
      if i >= endI then
        state
      else
        foldR (accFn state i) (i + 1) endI

    let (startI, endI) = range
    foldR state startI endI

  // fold over the range [start, end]  (i.e. including the end)
  let foldInclusive (accFn: 'State -> int -> 'State) (state: 'State) (range: Range) =
    let rec foldR state i endI =
      if i > endI then
        state
      else
        foldR (accFn state i) (i + 1) endI

    let (startI, endI) = range
    foldR state startI endI

  let map (fn: int -> 'T) (range: Range): array<'T> =
    let (startI, endI) = range
    let length = endI - startI + 1
    if length < 0 then
      failwith "The start of the range must not exceed the end of the range."
    else
      Array.init length (fun i -> fn (i + startI))

  (* Parallel versions *)

  let piter (fn: int -> unit) (range: Range): unit =
    let (startI, endI) = range
    Parallel.For(startI, endI, (fun i -> fn i)) |> ignore

  let pmap (fn: int -> 'T) (range: Range): array<'T> =
    let (startI, endI) = range
    let length = endI - startI + 1
    if length < 0 then
      failwith "The start of the range must not exceed the end of the range."
    else
      Array.Parallel.init length (fun i -> fn (i + startI))

module Option =
  let getOrElse (defaultValue: 't) (firstChoiceValue: Option<'t>): 't =
    match firstChoiceValue with
    | Some value -> value
    | None -> defaultValue

module Math =
  module Random =
    let ThreadLocalRandom = new Threading.ThreadLocal<Random>(fun () -> new Random(Threading.Thread.CurrentThread.ManagedThreadId))

    let randomInt upperBoundExclusive: int =
      let rnd = ThreadLocalRandom.Value
      rnd.Next(upperBoundExclusive)

    let randomInts upperBoundExclusive n: array<int> =
      let rnd = ThreadLocalRandom.Value
      Array.init n (fun _ -> rnd.Next(upperBoundExclusive))

    let randomIntsBetween lowerBound upperBoundExclusive n: array<int> =
      let range = upperBoundExclusive - lowerBound
      randomInts range n |> Array.map (fun offset -> lowerBound + offset)
  
    let randomDecimalsBetween lowerBound upperBoundExclusive n: array<decimal> = randomIntsBetween lowerBound upperBoundExclusive n |> Array.map decimal

  module Int =
    let pow value exp: int = 
      if exp >= 0 then
        Range.fold (fun memo i -> memo * value) 1 (0, exp)
      else
        failwith "ints cannot be raised to a negative power."

  module Double =
    let roundTo (fractionalDigits: int) (d: double): double = System.Math.Round(d, fractionalDigits)

  module Decimal = 
    let Zero = 0M
    let One = 1M
    let Billionth = 0.000000001m

    let ceil (d: decimal): decimal = System.Math.Ceiling(d)

    let floor (d: decimal): decimal = System.Math.Floor(d)

module Stats =
  module Sample =
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
              let i = Math.Decimal.floor h         // i is a 0-based index into sortedXs   (smaller index to interpolate between)
              let j = Math.Decimal.ceil h          // j is a 0-based index into sortedXs   (larger index to interpolate between)
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
              let i = int (Math.Decimal.floor h)   // i is a 0-based index into sortedXs
              getIthX i
            )
            hs

      // implementation based on description of R-1 at http://en.wikipedia.org/wiki/Quantile#Estimating_the_quantiles_of_a_population
      let quantilesR1 (interpolate: bool) (isSorted: bool) (percentages: array<decimal>) (xs: array<decimal>): array<decimal> =
        quantiles 
          (fun (n: decimal) (p: decimal) -> if p = 0m then 1m else n * p + 0.5m)
          (fun (getIthX: (int -> decimal)) (h: decimal) -> getIthX (int (Math.Decimal.ceil (h - 0.5m))))
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
            let floorHDec = Math.Decimal.floor h
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

(*
 * returns an array of sampling distributions s.t. each sampling distribution is created by 
 * constructing <nSamples> samples, each consisting of <nObservationsPerSample> observations, then
 * running each sample through the <multiStatisticFn> function, which produces an array of statistics for that sample.
 * The ith sampling distribution consists of the <nSamples> sample statistics produced by extracting the ith sample statistic
 * from the array returned by the application of the <multiStatisticFn> function over all the <nSamples> samples.
 * For example:
 *   buildSamplingDistributionsFromOneMultiStatisticFn 1000 100 buildSample (fun sample -> [| mean sample; median sample |])
 * returns 2 sampling distributions - a sampling distributions of the mean and a sampling distribution of the median
 *)
let buildSamplingDistributionsFromOneMultiStatisticFn
      nSamples 
      nObservationsPerSample 
      (buildSampleFn: int -> array<decimal>) 
      (multiStatisticFn: array<decimal> -> array<decimal>)
      : array<array<decimal>> =
  let diagnosticSample = [|1m; 2m; 3m|]
  let numberOfSamplingDistributions = multiStatisticFn diagnosticSample |> Array.length
  let samplingDistributions = Array.init numberOfSamplingDistributions (fun _ -> Array.create nSamples 0m)

  (0, nSamples - 1)
  |> Range.piter
    (fun sampleIndex ->
      let sample = buildSampleFn nObservationsPerSample
      let sampleStatistics = multiStatisticFn sample
      sampleStatistics |> Array.iteri (fun i sampleStatistic -> samplingDistributions.[i].[sampleIndex] <- sampleStatistic)
    )

  samplingDistributions

(*
 * Takes a sample of, for example, weekly return observations (i.e. observedReturns) and constructs <sampleSizeN> 
 * sequences, each consisting of <numberOfTimePeriodsToConcatenate> randomly chosen return observations from <observedReturns>.
 * The randomly chosen return observations in each sequence are multiplied together in the order the observations were chosen,
 * thus producing a cumulative return observation per sequence.
 * The resulting cumulative return observations are then returned from this function as a cumulative return sample.
 *
 * For example,
 *   let monthlyReturns = [| 1.1; 1.2; 0.7; |]
 *   buildCumulativeReturnSample monthlyReturns 12 1000
 * would return a sample of 1000 simulated yearly return observations, given an initial sample of 3 monthly return observations
 *)
let buildCumulativeReturnSample (observedReturns: array<decimal>) numberOfTimePeriodsToConcatenate sampleSizeN: array<decimal> =
  let rnd = new Random(int DateTime.Now.Ticks)
  let observedReturnsLength = Array.length observedReturns
  
  let rec calculateCumulativeReturnR (rnd: Random) nObservationsToMultiply (observedReturns: array<decimal>) (observedReturnsLength: int) (acc: decimal) i: decimal =
    if i = nObservationsToMultiply then
      acc
    else
      let newAcc = acc * observedReturns.[rnd.Next(0, observedReturnsLength)]
      calculateCumulativeReturnR rnd nObservationsToMultiply observedReturns observedReturnsLength newAcc (i + 1)

  (1, sampleSizeN) |> Range.map (fun _ -> calculateCumulativeReturnR rnd numberOfTimePeriodsToConcatenate observedReturns observedReturnsLength 1m 0)


// returns a sample of <sampleSize> simulated yearly return observations, given an initial sample of 3 monthly return observations
let build1YearReturnSampleFromWeeklyReturns weeklyReturns sampleSize: array<decimal> = 
  buildCumulativeReturnSample weeklyReturns 52 sampleSize

// computes the following sample statistics of a single sample (in the following order): 
// mean, min, max, 1st %ile, 5th %ile, 10th %ile, 15th %ile, ..., 95th %ile, and 99th %ile
let computeSampleStatistics (sample: array<decimal>): array<decimal> = 
  let onlineMeanMinMax = new Stats.Sample.OnlineMeanMinMax()
  onlineMeanMinMax.pushAll sample

  let mean = onlineMeanMinMax.mean
  let min = onlineMeanMinMax.min |> Option.getOrElse 0m
  let max = onlineMeanMinMax.max |> Option.getOrElse 0m

  Array.append
    [| mean; min; max |]
    (Stats.Sample.Array.percentiles [|1m; 5m; 10m; 15m; 20m; 25m; 30m; 35m; 40m; 45m; 50m; 55m; 60m; 65m; 70m; 75m; 80m; 85m; 90m; 95m; 99m|] sample)

let computeAndSamplingDistributions weeklyReturns numberOfSamples numberOfObservationsPerSample: unit =
  let t1 = DateTime.Now

  let samplingDistributions = 
    buildSamplingDistributionsFromOneMultiStatisticFn 
      numberOfSamples 
      numberOfObservationsPerSample 
      (build1YearReturnSampleFromWeeklyReturns weeklyReturns)
      computeSampleStatistics
  
  let t2 = DateTime.Now
  printfn "Building sampling distributions from %i samples, each containing %i observations, took %A seconds." 
    numberOfSamples 
    numberOfObservationsPerSample
    (t2 - t1)

[<EntryPoint>]
let main argv = 
  (* let returnsF = [| 23.2; 15.1; 2.4; -3.9; 25.1; 7.6; -2.8;  -12.6; -3.5; 5.6; -2.2; 11.8; 16.5; 30.9; 5.0; 35.9; -2.8; -25.5; 21.3;  9.4; 15.0; 22.5; -5.5; 18.1; -13.7; 20.3; -3.9; 10.5; -0.3; 0.2; -11.5;  31.6; -13.3; 14.4; 1.1; 12.9; 5.0; -16.8; -0.7; 1.3; 0.2; 18.9; 15.5; -11.7; 9.2; -11.8; -25.6 |] *)
  (* let returnsF = [| 0.2; 18.9; 15.5; -11.7; 9.2; -11.8 |] *)
  (* let returnsF = [| -16.8; 0.2; 18.9; 15.5; -11.7; 9.2; -11.8 |] *)
  let returnsF = [| 0.2; 18.9; 15.5; -11.7; 9.2; -11.8; -17.2 |]
  let returns = returnsF |> Array.map (fun r -> (decimal r + 100m) / 100m)

  computeAndSamplingDistributions returns 10000 10000

  0