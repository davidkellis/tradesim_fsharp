module dke.tradesim.TrialSetStats

open FSharpx

open Core
open Stats
open Database

type Distribution = {
  n: int
  average: decimal
  min: decimal
  max: decimal
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
}

let YieldExtractor trialResult: Option<decimal> = trialResult.trialYield
let MfeExtractor trialResult: Option<decimal> = trialResult.mfe
let MaeExtractor trialResult: Option<decimal> = trialResult.mae
let DailyStdDevExtractor trialResult: Option<decimal> = trialResult.dailyStdDev

let buildDistribution sampleValues: Distribution =
  let onlineVariance = new Stats.Sample.OnlineVariance()
  onlineVariance.pushAll sampleValues
  let percentiles = Stats.Sample.percentiles [5m; 10m; 15m; 20m; 25m; 30m; 35m; 40m; 45m; 50m; 55m; 60m; 65m; 70m; 75m; 80m; 85m; 90m; 95m] sampleValues |> Seq.toArray
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

let SamplingDistributionType = 
  Mean
  | Min
  | Max

// implements the bootstrap method to build a sampling distribution of the <some statistic; e.g. mean, max, min, etc.)
// given a single sample of values
let buildSamplingDistribution samplingDistributionType originalSample: Distribution = 
  let bootstrapSamples = sample 1000 (Array.length originalSample) originalSample
  let newSample = 
    Array.map
      (fun sample -> 
        let onlineVariance = new Stats.Sample.OnlineVariance()
        onlineVariance.pushAll sample

        match samplingDistributionType with
        | Mean -> onlineVariance.mean
        | Min -> onlineVariance.min
        | Max -> onlineVariance.max
      )
      bootstrapSamples
  buildDistribution newSample

let buildSamplingDistributionFromTrialResults samplingDistributionType (valueExtractorFn: TrialResult -> Option<decimal>) (sample: seq<TrialResult>): Distribution =
  trialResults |> Seq.flatMapO valueExtractorFn |> (buildSamplingDistribution samplingDistributionType)


let printDistribution distribution: unit =
  printfn "n = %i" distribution.n
  printfn "mean = %M" distribution.average
  printfn "%s%%" <| String.joinInts "\t" [ 0 .. 5 .. 100 ]
  printf "%M\t" <| distribution.min
  printf "%M\t" <| distribution.percentile5
  printf "%M\t" <| distribution.percentile10
  printf "%M\t" <| distribution.percentile15
  printf "%M\t" <| distribution.percentile20
  printf "%M\t" <| distribution.percentile25
  printf "%M\t" <| distribution.percentile30
  printf "%M\t" <| distribution.percentile35
  printf "%M\t" <| distribution.percentile40
  printf "%M\t" <| distribution.percentile45
  printf "%M\t" <| distribution.percentile50
  printf "%M\t" <| distribution.percentile55
  printf "%M\t" <| distribution.percentile60
  printf "%M\t" <| distribution.percentile65
  printf "%M\t" <| distribution.percentile70
  printf "%M\t" <| distribution.percentile75
  printf "%M\t" <| distribution.percentile80
  printf "%M\t" <| distribution.percentile85
  printf "%M\t" <| distribution.percentile90
  printf "%M\t" <| distribution.percentile95
  printfn "%M" <| distribution.max

let printReport (trialResults: seq<TrialResult>): unit =
  let yieldDistribution = buildDistribution YieldExtractor trialResults
  let mfeDistribution = buildDistribution MfeExtractor trialResults
  let maeDistribution = buildDistribution MaeExtractor trialResults
  let dailyStdDevDistribution = buildDistribution DailyStdDevExtractor trialResults

  printfn "Trial Results"
  printfn "Yield Distribution"
  printDistribution yieldDistribution
  printfn "Maximum Favorable Excursion Distribution"
  printDistribution mfeDistribution
  printfn "Maximum Adverse Excursion Distribution"
  printDistribution maeDistribution
  printfn "Daily Std. Dev. Distribution"
  printDistribution dailyStdDevDistribution

let buildMissingTrialSetDistributions connectionString trialAttribute: unit =
  // figure out which trials set distributions are missing
  // 1. figure out the greatest end date
  // build each missing trial set distribution
  ()