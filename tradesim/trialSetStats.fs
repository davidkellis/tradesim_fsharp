module dke.tradesim.TrialSetStats

open FSharpx

open Core
open Math
open Stats
open Database

type Distribution = {
  n: int
  average: decimal
  min: decimal
  max: decimal
  percentile1: decimal
  percentile5: decimal
  percentile10: decimal
  percentile15: decimal
  percentile20: decimal
  percentile25: decimal
  percentile30: decimal
  percentile35: decimal
  percentile40: decimal
  percentile45: decimal
  percentile50: decimal
  percentile55: decimal
  percentile60: decimal
  percentile65: decimal
  percentile70: decimal
  percentile75: decimal
  percentile80: decimal
  percentile85: decimal
  percentile90: decimal
  percentile95: decimal
  percentile99: decimal
}

let YieldExtractor trialResult: Option<decimal> = trialResult.trialYield
let MfeExtractor trialResult: Option<decimal> = trialResult.mfe
let MaeExtractor trialResult: Option<decimal> = trialResult.mae
let DailyStdDevExtractor trialResult: Option<decimal> = trialResult.dailyStdDev

let buildDistribution sampleValues: Distribution =
  let onlineVariance = new Stats.Sample.OnlineVariance()
  onlineVariance.pushAll sampleValues
  let percentiles = Stats.Sample.Seq.percentiles [5m; 10m; 15m; 20m; 25m; 30m; 35m; 40m; 45m; 50m; 55m; 60m; 65m; 70m; 75m; 80m; 85m; 90m; 95m] sampleValues |> Seq.toArray
  {
    n = onlineVariance.n |> int
    average = onlineVariance.mean
    min = onlineVariance.min |> Option.getOrElse 0m
    max = onlineVariance.max |> Option.getOrElse 0m
    percentile5 = percentiles.[0]
    percentile10 = percentiles.[1]
    percentile15 = percentiles.[2]
    percentile20 = percentiles.[3]
    percentile25 = percentiles.[4]
    percentile30 = percentiles.[5]
    percentile35 = percentiles.[6]
    percentile40 = percentiles.[7]
    percentile45 = percentiles.[8]
    percentile50 = percentiles.[9]
    percentile55 = percentiles.[10]
    percentile60 = percentiles.[11]
    percentile65 = percentiles.[12]
    percentile70 = percentiles.[13]
    percentile75 = percentiles.[14]
    percentile80 = percentiles.[15]
    percentile85 = percentiles.[16]
    percentile90 = percentiles.[17]
    percentile95 = percentiles.[18]
  }

let buildDistributionFromTrialResults (valueExtractorFn: TrialResult -> Option<decimal>) (trialResults: seq<TrialResult>): Distribution =
  trialResults |> Seq.flatMapO valueExtractorFn |> buildDistribution

type SamplingDistributionType = Mean | StdDev | Min | Max | Percentile of int

// implements the bootstrap method to build a sampling distribution of the <some statistic; e.g. mean, standard deviation, max, min, percentile, etc.)
// given a single sample of values
let buildSamplingDistribution samplingDistributionType originalSample: Distribution = 
  let bootstrapSamples = buildBootstrapSamples 1000 originalSample
  let sampleStatisticFn = 
    match samplingDistributionType with
    | Mean -> Sample.Seq.mean
    | StdDev -> Sample.stdDev
    | Min -> Array.reduce min
    | Max -> Array.reduce max
    | Percentile p -> fun bootstrapSample -> Stats.Sample.Seq.percentiles [p |> decimal] bootstrapSample |> Seq.head

  let samplingDistribution = Array.map sampleStatisticFn bootstrapSamples
  buildDistribution samplingDistribution

let buildSamplingDistributionFromTrialResults samplingDistributionType (valueExtractorFn: TrialResult -> Option<decimal>) (trialResults: seq<TrialResult>): Distribution =
  trialResults |> Seq.flatMapO valueExtractorFn |> Seq.toArray |> buildSamplingDistribution samplingDistributionType


let printDistribution distribution: unit =
  printfn "n = %i" distribution.n
  printfn "mean = %M" <| Decimal.roundTo 4 distribution.average
  printfn "%s%%" <| String.joinInts "%\t" [ 0 .. 5 .. 100 ]
  printf "%M\t" <| Decimal.roundTo 3 distribution.min
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile5
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile10
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile15
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile20
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile25
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile30
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile35
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile40
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile45
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile50
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile55
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile60
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile65
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile70
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile75
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile80
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile85
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile90
  printf "%M\t" <| Decimal.roundTo 3 distribution.percentile95
  printfn "%M" <| Decimal.roundTo 3 distribution.max

let printReport (trialResults: seq<TrialResult>): unit =
  let yieldValues = trialResults |> Seq.flatMapO YieldExtractor |> Seq.toArray
  let mfeValues = trialResults |> Seq.flatMapO MfeExtractor |> Seq.toArray
  let maeValues = trialResults |> Seq.flatMapO MaeExtractor |> Seq.toArray
  let dailyStdDevValues = trialResults |> Seq.flatMapO DailyStdDevExtractor |> Seq.toArray

  let yieldDistribution = yieldValues |> buildDistribution
  let mfeDistribution = mfeValues |> buildDistribution
  let maeDistribution = maeValues |> buildDistribution
  let dailyStdDevDistribution = dailyStdDevValues |> buildDistribution

  let samplingDistOfMeanYield = buildSamplingDistribution Mean yieldValues
  let samplingDistOfMeanMfe = buildSamplingDistribution Mean mfeValues
  let samplingDistOfMeanMae = buildSamplingDistribution Mean maeValues
  let samplingDistOfMeanDailyStdDev = buildSamplingDistribution Mean dailyStdDevValues

  printfn "Trial Results Summary"
  printfn "Yield Distribution"
  printDistribution yieldDistribution
  printDistribution samplingDistOfMeanYield
  printfn "Maximum Favorable Excursion Distribution"
  printDistribution mfeDistribution
  printDistribution samplingDistOfMeanMfe
  printfn "Maximum Adverse Excursion Distribution"
  printDistribution maeDistribution
  printDistribution samplingDistOfMeanMae
//  printfn "Daily Std. Dev. Distribution"
//  printDistribution dailyStdDevDistribution
//  printDistribution samplingDistOfMeanDailyStdDev

let buildMissingTrialSetDistributions connectionString trialAttribute: unit =
  // figure out which trials set distributions are missing
  // 1. figure out the greatest end date
  // build each missing trial set distribution
  ()