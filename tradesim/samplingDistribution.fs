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

module Decimal =
  (*
   * returns an array of sampling distributions s.t. each sampling distribution is created by 
   * constructing <nSamples> samples, each consisting of <nObservationsPerSample> observations, then
   * running each sample through the <multiStatisticFn> function, which produces an array of statistics for that sample.
   * The ith sampling distribution consists of the <nSamples> sample statistics produced by extracting the ith sample statistic
   * from the array returned by the application of the <multiStatisticFn> function over all the <nSamples> samples.
   * NOTE: numberOfStatisticsReturnedByMultiStatisticFn needs to be equal to the length of the array returned by multiStatisticFn
   * For example:
   *   buildSamplingDistributionsFromOneMultiStatisticFn 1000 100 buildSample (fun sample -> [| mean sample; median sample |]) 2
   * returns 2 sampling distributions - a sampling distributions of the mean and a sampling distribution of the median
   *)
  let buildSamplingDistributionsFromOneMultiStatisticFn
        nSamples 
        nObservationsPerSample 
        (buildSampleFn: int -> array<decimal>) 
        (multiStatisticFn: array<decimal> -> array<decimal>)
        (numberOfStatisticsReturnedByMultiStatisticFn: int)
        : array<array<decimal>> =
    let numberOfSamplingDistributions = numberOfStatisticsReturnedByMultiStatisticFn
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
  let periodizeReturn (numberOfShortPeriodsUsedToCalculateValue: int) (numberOfShortPeriodsInLongPeriod: int) (returnValue: decimal): decimal =
    Decimal.pow (Decimal.nthRoot returnValue numberOfShortPeriodsUsedToCalculateValue) numberOfShortPeriodsInLongPeriod

  let annualizeMonthlyReturn months (returnValue: decimal): decimal = periodizeReturn months 12 returnValue
  let annualizeWeeklyReturn weeks (returnValue: decimal): decimal = periodizeReturn weeks 52 returnValue

  let calculateCumulativeReturn (returns: array<decimal>): decimal = returns |> Array.reduce (fun acc d -> acc * d)

  let annualizeMonthlyCumulativeReturn (returns: array<decimal>): decimal = calculateCumulativeReturn returns |> annualizeMonthlyReturn returns.Length
  let annualizeWeeklyCumulativeReturn (returns: array<decimal>): decimal = calculateCumulativeReturn returns |> annualizeWeeklyReturn returns.Length

  let buildSample (nObservations: int) (buildObservationFn: unit -> 'T): array<'T> = Array.init nObservations (fun i -> buildObservationFn () )
  *)

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


module Double =
  (*
   * returns an array of sampling distributions s.t. each sampling distribution is created by 
   * constructing <nSamples> samples, each consisting of <nObservationsPerSample> observations, then
   * running each sample through the <multiStatisticFn> function, which produces an array of statistics for that sample.
   * The ith sampling distribution consists of the <nSamples> sample statistics produced by extracting the ith sample statistic
   * from the array returned by the application of the <multiStatisticFn> function over all the <nSamples> samples.
   * NOTE: numberOfStatisticsReturnedByMultiStatisticFn needs to be equal to the length of the array returned by multiStatisticFn
   * For example:
   *   buildSamplingDistributionsFromOneMultiStatisticFn 1000 100 buildSample (fun sample -> [| mean sample; median sample |]) 2
   * returns 2 sampling distributions - a sampling distributions of the mean and a sampling distribution of the median
   *)
  let buildSamplingDistributionsFromOneMultiStatisticFn
        nSamples 
        nObservationsPerSample 
        (buildSampleFn: int -> array<double>) 
        (multiStatisticFn: array<double> -> array<double>)
        (numberOfStatisticsReturnedByMultiStatisticFn: int)
        : array<array<double>> =
    let numberOfSamplingDistributions = numberOfStatisticsReturnedByMultiStatisticFn
    let samplingDistributions = Array.init numberOfSamplingDistributions (fun _ -> Array.create nSamples 0.0)

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
  let buildCumulativeReturnSample (observedReturns: array<double>) numberOfTimePeriodsToConcatenate sampleSizeN: array<double> =
    let rnd = new Random(int DateTime.Now.Ticks)
    let observedReturnsLength = Array.length observedReturns
    
    let rec calculateCumulativeReturnR (rnd: Random) nObservationsToMultiply (observedReturns: array<double>) (observedReturnsLength: int) (acc: double) i: double =
      if i = nObservationsToMultiply then
        acc
      else
        let newAcc = acc * observedReturns.[rnd.Next(0, observedReturnsLength)]
        calculateCumulativeReturnR rnd nObservationsToMultiply observedReturns observedReturnsLength newAcc (i + 1)

    (1, sampleSizeN) |> Range.map (fun _ -> calculateCumulativeReturnR rnd numberOfTimePeriodsToConcatenate observedReturns observedReturnsLength 1.0 0)
