module dke.tradesim.SamplingDistribution

open System
open System.Threading.Tasks

open Math
open Stats

(*
  // returns array of sampling distributions s.t. the ith sampling distribution is the sampling distribution of the statistic computed by compute_sample_statistic_fns[i]
  let buildSamplingDistributions nSamples nObservationsPerSample buildSampleFn computeSampleStatisticFns =
    let samplingDistributions = Array.init (Array.length computeSampleStatisticFns) (fun _ -> Array.create nSamples 0m)

    for sampleIndex = 0 to (nSamples - 1) do
      let sample = buildSampleFn nObservationsPerSample
      computeSampleStatisticFns |> Seq.iteri (fun i computeSampleStatisticFn -> samplingDistributions.[i].[sampleIndex] <- computeSampleStatisticFn sample)

    samplingDistributions
*)

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

let periodizeReturn (numberOfShortPeriodsUsedToCalculateValue: int) (numberOfShortPeriodsInLongPeriod: int) (returnValue: decimal): decimal =
  Decimal.pow (Decimal.nthRoot returnValue numberOfShortPeriodsUsedToCalculateValue) numberOfShortPeriodsInLongPeriod

let annualizeMonthlyReturn months (returnValue: decimal): decimal = periodizeReturn months 12 returnValue
let annualizeWeeklyReturn weeks (returnValue: decimal): decimal = periodizeReturn weeks 52 returnValue

let calculateCumulativeReturn (returns: array<decimal>): decimal = returns |> Array.reduce (fun acc d -> acc * d)

let annualizeMonthlyCumulativeReturn (returns: array<decimal>): decimal = calculateCumulativeReturn returns |> annualizeMonthlyReturn returns.Length
let annualizeWeeklyCumulativeReturn (returns: array<decimal>): decimal = calculateCumulativeReturn returns |> annualizeWeeklyReturn returns.Length

let buildSample (nObservations: int) (buildObservationFn: unit -> 'T): array<'T> = Array.init nObservations (fun i -> buildObservationFn () )

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
    (Sample.Array.percentiles [|1m; 5m; 10m; 15m; 20m; 25m; 30m; 35m; 40m; 45m; 50m; 55m; 60m; 65m; 70m; 75m; 80m; 85m; 90m; 95m; 99m|] sample)
