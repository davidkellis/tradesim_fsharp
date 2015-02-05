module dke.returnStats.computeMissingSamplingDistributionsAsDoubles

open System

open jack
open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharpx
open FSharpx.Collections
open NodaTime
open Npgsql

open dke.tradesim
open Core
open Database
open Database.Postgres
open Logging
open Time
open Trial
open Quotes
open Schedule
open Stats
open SamplingDistribution
open strategies.BuyAndHold

type TrialSetDistributionReference = {
  trialSetDistributionId: int
}

type TrialSetDistribution = {
  id: int option
  attribute: string
  startTime: ZonedDateTime
  endTime: ZonedDateTime
  distribution: array<double>

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

type SampleStatistic = {
  id: int option
  name: string
}

type SamplingDistribution = {
  id: int option
  trialSetDistributionId: int
  sampleStatisticId: int
  distribution: array<double>

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

let decodeMessage message: TrialSetDistributionReference =
  let json = JsonValue.Parse(message)
  let trialSetDistributionId = json?trial_set_distribution_id.AsInteger()
  {
    trialSetDistributionId = trialSetDistributionId
  }

let toSampleStatistic (reader: NpgsqlDataReader): SampleStatistic =
  { 
    id = dbOptInt reader "id"
    name = dbGetStr reader "name"
  }

let toTrialSetDistribution (reader: NpgsqlDataReader): TrialSetDistribution =
  { 
    id = dbOptInt reader "id"
    attribute = dbGetStr reader "attribute"
    startTime = dbGetLong reader "start_time" |> timestampToDatetime
    endTime = dbGetLong reader "end_time" |> timestampToDatetime
    distribution = dbGetBytes reader "distribution" |> DoubleList.decode 3
    
    n = dbGetInt reader "n"
    average = dbGetDecimal reader "average"
    min = dbGetDecimal reader "min"
    max = dbGetDecimal reader "max"
    percentile1  = dbGetDecimal reader "percentile_1"
    percentile5  = dbGetDecimal reader "percentile_5"
    percentile10 = dbGetDecimal reader "percentile_10"
    percentile15 = dbGetDecimal reader "percentile_15"
    percentile20 = dbGetDecimal reader "percentile_20"
    percentile25 = dbGetDecimal reader "percentile_25"
    percentile30 = dbGetDecimal reader "percentile_30"
    percentile35 = dbGetDecimal reader "percentile_35"
    percentile40 = dbGetDecimal reader "percentile_40"
    percentile45 = dbGetDecimal reader "percentile_45"
    percentile50 = dbGetDecimal reader "percentile_50"
    percentile55 = dbGetDecimal reader "percentile_55"
    percentile60 = dbGetDecimal reader "percentile_60"
    percentile65 = dbGetDecimal reader "percentile_65"
    percentile70 = dbGetDecimal reader "percentile_70"
    percentile75 = dbGetDecimal reader "percentile_75"
    percentile80 = dbGetDecimal reader "percentile_80"
    percentile85 = dbGetDecimal reader "percentile_85"
    percentile90 = dbGetDecimal reader "percentile_90"
    percentile95 = dbGetDecimal reader "percentile_95"
    percentile99 = dbGetDecimal reader "percentile_99"
  }

let lookupTrialSetDistribution connectionString id: TrialSetDistribution =
  let sql = """
    select *
    from trial_set_distributions
    where id = @id
  """
  query
    connectionString
    sql 
    [intParam "id" id]
    toTrialSetDistribution
  |> Seq.firstOption |> Option.get

let insertSamplingDistribution connectionString (samplingDistribution: SamplingDistribution): SamplingDistribution = 
  let sql = """
    insert into sampling_distributions
    (
      trial_set_distribution_id, 
      sample_statistic_id, 
      distribution, 
      n, 
      average, 
      min, 
      max,
      percentile_1,
      percentile_5,
      percentile_10,
      percentile_15,
      percentile_20,
      percentile_25,
      percentile_30,
      percentile_35,
      percentile_40,
      percentile_45,
      percentile_50,
      percentile_55,
      percentile_60,
      percentile_65,
      percentile_70,
      percentile_75,
      percentile_80,
      percentile_85,
      percentile_90,
      percentile_95,
      percentile_99
    )
    values
    (
      @trialSetDistributionId,
      @sampleStatisticId,
      @distribution,
      @n,
      @average,
      @min,
      @max,
      @percentile1,
      @percentile5,
      @percentile10,
      @percentile15,
      @percentile20,
      @percentile25,
      @percentile30,
      @percentile35,
      @percentile40,
      @percentile45,
      @percentile50,
      @percentile55,
      @percentile60,
      @percentile65,
      @percentile70,
      @percentile75,
      @percentile80,
      @percentile85,
      @percentile90,
      @percentile95,
      @percentile99
    )
    returning id;
  """
  let id =
    insertReturningId
      connectionString
      sql
      [
        intParam "trialSetDistributionId" samplingDistribution.trialSetDistributionId;
        intParam "sampleStatisticId" samplingDistribution.sampleStatisticId;
        byteArrayParam "distribution" (DoubleList.encode 3 samplingDistribution.distribution);
        intParam "n" samplingDistribution.n;
        decimalParam "average" samplingDistribution.average;
        decimalParam "min" samplingDistribution.min;
        decimalParam "max" samplingDistribution.max;
        decimalParam "percentile1" samplingDistribution.percentile1;
        decimalParam "percentile5" samplingDistribution.percentile5;
        decimalParam "percentile10" samplingDistribution.percentile10;
        decimalParam "percentile15" samplingDistribution.percentile15;
        decimalParam "percentile20" samplingDistribution.percentile20;
        decimalParam "percentile25" samplingDistribution.percentile25;
        decimalParam "percentile30" samplingDistribution.percentile30;
        decimalParam "percentile35" samplingDistribution.percentile35;
        decimalParam "percentile40" samplingDistribution.percentile40;
        decimalParam "percentile45" samplingDistribution.percentile45;
        decimalParam "percentile50" samplingDistribution.percentile50;
        decimalParam "percentile55" samplingDistribution.percentile55;
        decimalParam "percentile60" samplingDistribution.percentile60;
        decimalParam "percentile65" samplingDistribution.percentile65;
        decimalParam "percentile70" samplingDistribution.percentile70;
        decimalParam "percentile75" samplingDistribution.percentile75;
        decimalParam "percentile80" samplingDistribution.percentile80;
        decimalParam "percentile85" samplingDistribution.percentile85;
        decimalParam "percentile90" samplingDistribution.percentile90;
        decimalParam "percentile95" samplingDistribution.percentile95;
        decimalParam "percentile99" samplingDistribution.percentile99
      ]
  {samplingDistribution with id = id}

let loadSampleStatistic connectionString name: SampleStatistic =
  let sql = """
    select *
    from sample_statistics
    where name = @name
  """
  query
    connectionString
    sql 
    [stringParam "name" name]
    toSampleStatistic
  |> Seq.firstOption |> Option.get

let allSampleStatistics connectionString = [|
  loadSampleStatistic connectionString "mean";
  loadSampleStatistic connectionString "min";
  loadSampleStatistic connectionString "max";
  loadSampleStatistic connectionString "percentile1";
  loadSampleStatistic connectionString "percentile5";
  loadSampleStatistic connectionString "percentile10";
  loadSampleStatistic connectionString "percentile15";
  loadSampleStatistic connectionString "percentile20";
  loadSampleStatistic connectionString "percentile25";
  loadSampleStatistic connectionString "percentile30";
  loadSampleStatistic connectionString "percentile35";
  loadSampleStatistic connectionString "percentile40";
  loadSampleStatistic connectionString "percentile45";
  loadSampleStatistic connectionString "percentile50";
  loadSampleStatistic connectionString "percentile55";
  loadSampleStatistic connectionString "percentile60";
  loadSampleStatistic connectionString "percentile65";
  loadSampleStatistic connectionString "percentile70";
  loadSampleStatistic connectionString "percentile75";
  loadSampleStatistic connectionString "percentile80";
  loadSampleStatistic connectionString "percentile85";
  loadSampleStatistic connectionString "percentile90";
  loadSampleStatistic connectionString "percentile95";
  loadSampleStatistic connectionString "percentile99"
|]

// computes the following sample statistics of a single sample (in the following order): 
// mean, min, max, 1st %ile, 5th %ile, 10th %ile, 15th %ile, ..., 95th %ile, and 99th %ile
let computeSampleStatistics (sample: array<double>): array<double> = 
  let onlineMeanMinMax = new Sample.Double.OnlineMeanMinMax()
  onlineMeanMinMax.pushAll sample

  let mean = onlineMeanMinMax.mean
  let min = onlineMeanMinMax.min |> Option.getOrElse 0.0
  let max = onlineMeanMinMax.max |> Option.getOrElse 0.0

  Array.append
    [| mean; min; max |]
    (Sample.Double.Array.percentiles [|1.0; 5.0; 10.0; 15.0; 20.0; 25.0; 30.0; 35.0; 40.0; 45.0; 50.0; 55.0; 60.0; 65.0; 70.0; 75.0; 80.0; 85.0; 90.0; 95.0; 99.0|] sample)

let buildSamplingDistributionRecord connectionString trialSetDistributionId sampleStatisticId (samplingDistribution: array<double>): SamplingDistribution =
  let sampleStatistics = computeSampleStatistics samplingDistribution
  let n = Array.length samplingDistribution
  let mean = sampleStatistics.[0] |> decimal
  let min = sampleStatistics.[1] |> decimal
  let max = sampleStatistics.[2] |> decimal
  let percentile1 = sampleStatistics.[3] |> decimal
  let percentile5 = sampleStatistics.[4] |> decimal
  let percentile10 = sampleStatistics.[5] |> decimal
  let percentile15 = sampleStatistics.[6] |> decimal
  let percentile20 = sampleStatistics.[7] |> decimal
  let percentile25 = sampleStatistics.[8] |> decimal
  let percentile30 = sampleStatistics.[9] |> decimal
  let percentile35 = sampleStatistics.[10] |> decimal
  let percentile40 = sampleStatistics.[11] |> decimal
  let percentile45 = sampleStatistics.[12] |> decimal
  let percentile50 = sampleStatistics.[13] |> decimal
  let percentile55 = sampleStatistics.[14] |> decimal
  let percentile60 = sampleStatistics.[15] |> decimal
  let percentile65 = sampleStatistics.[16] |> decimal
  let percentile70 = sampleStatistics.[17] |> decimal
  let percentile75 = sampleStatistics.[18] |> decimal
  let percentile80 = sampleStatistics.[19] |> decimal
  let percentile85 = sampleStatistics.[20] |> decimal
  let percentile90 = sampleStatistics.[21] |> decimal
  let percentile95 = sampleStatistics.[22] |> decimal
  let percentile99 = sampleStatistics.[23] |> decimal

  {
    id = None
    trialSetDistributionId = trialSetDistributionId
    sampleStatisticId = sampleStatisticId
    distribution = samplingDistribution

    n = n
    average = mean
    min = min
    max = max
    percentile1 = percentile1
    percentile5 = percentile5
    percentile10 = percentile10
    percentile15 = percentile15
    percentile20 = percentile20
    percentile25 = percentile25
    percentile30 = percentile30
    percentile35 = percentile35
    percentile40 = percentile40
    percentile45 = percentile45
    percentile50 = percentile50
    percentile55 = percentile55
    percentile60 = percentile60
    percentile65 = percentile65
    percentile70 = percentile70
    percentile75 = percentile75
    percentile80 = percentile80
    percentile85 = percentile85
    percentile90 = percentile90
    percentile95 = percentile95
    percentile99 = percentile99
  }

// returns a sample of <sampleSize> simulated yearly return observations, given an initial sample of 3 monthly return observations
let build1YearReturnSampleFromWeeklyReturns weeklyReturns sampleSize: array<double> = 
  Double.buildCumulativeReturnSample weeklyReturns 52 sampleSize

let computeAndStoreSamplingDistributions connectionString trialSetDistributionId: unit =
  let t1 = DateTime.Now
  
  let trialSetDistribution = lookupTrialSetDistribution connectionString trialSetDistributionId
  let weeklyReturns = trialSetDistribution.distribution
  
  let numberOfSamples = 5000
  let numberOfObservationsPerSample = 5000
  let sampleStatistics = allSampleStatistics connectionString
  let sampleStatisticsCount = computeSampleStatistics [| 1.0; 2.0; 3.0 |] |> Array.length
  let samplingDistributions = 
    Double.buildSamplingDistributionsFromOneMultiStatisticFn 
      numberOfSamples 
      numberOfObservationsPerSample 
      (build1YearReturnSampleFromWeeklyReturns weeklyReturns)
      computeSampleStatistics
      sampleStatisticsCount
  // NOTE: the sample statistics in <sampleStatistics> are in the same order as the statistics used to
  // construct the sampling distributions in <samplingDistributions>; in other words, they are parallel arrays
  let samplingDistributionsWithSampleStatistics = Array.zip samplingDistributions sampleStatistics
  
  let t2 = DateTime.Now
  printfn "Building sampling distributions for TrialSetDistribution %i from %i samples, each containing %i observations, took %A seconds." 
    trialSetDistributionId 
    numberOfSamples 
    numberOfObservationsPerSample (t2 - t1)
  
  // build SamplingDistribution objects from sampling distribution arrays stored in samplingDistributions
  let samplingDistributionRecords = 
    samplingDistributionsWithSampleStatistics
    |> Array.map (fun (samplingDistributionArray, sampleStatistic) -> 
      let sampleStatisticId = sampleStatistic.id |> Option.get
      buildSamplingDistributionRecord connectionString trialSetDistributionId sampleStatisticId samplingDistributionArray
    )

  // insert sampling distributions into database
  samplingDistributionRecords |> Array.map (insertSamplingDistribution connectionString)
  |> ignore

let run connectionString beanstalkdHost beanstalkdPort =
  info "Awaiting job from compute_missing_sampling_distributions queue"

  let client = Client.connect (beanstalkdHost, beanstalkdPort)
  client.watch "compute_missing_sampling_distributions" |> ignore

  let mutable keepLooping = true
  while keepLooping do
    let result = client.reserveWithTimeout 5
    match result with
    | Success (jobId, payload) ->
      printfn "jobId=%i  payload=%s" jobId payload

      let trialSetDistributionReference = decodeMessage payload
      
      // the assumption is that the specified trial set distribution has *no* associated sampling distributions, so we need to build them all.
      computeAndStoreSamplingDistributions connectionString trialSetDistributionReference.trialSetDistributionId

      client.delete jobId |> ignore
    | jack.Failure msg ->
      failwith msg
      keepLooping <- false

