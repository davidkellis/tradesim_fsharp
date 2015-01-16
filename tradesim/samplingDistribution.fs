module dke.tradesim.SamplingDistribution

open System
open System.Threading.Tasks

open Math
open Stats

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

let computeAllStats (sample: array<decimal>): array<decimal> = 
  Array.append
    [| Sample.Array.mean sample |]
    (Sample.Array.percentiles [|1m; 5m; 10m; 20m; 30m; 40m; 50m; 60m; 70m; 80m; 90m; 95m; 99m|] sample)
