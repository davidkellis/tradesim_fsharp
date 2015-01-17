module dke.returnStats.computeMissingSamplingDistributions

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
  endtime: ZonedDateTime
  distribution: array<decimal>

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

type SamplingDistribution = {
  id: int option
  trialSetDistributionId: int
  sampleStatistic: string
  distribution: array<decimal>

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

let toTrialSetDistribution (reader: NpgsqlDataReader): TrialSetDistribution =
  { 
    id = dbOptInt reader "id"
    attribute = dbGetStr reader "attribute"
    startTime = dbGetLong reader "start_time" |> timestampToDatetime
    endTime = dbGetLong reader "end_time" |> timestampToDatetime
    distribution = dbGetBytes reader "distribution" |> DecimalList.decode 3
    
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

let lookupTrialSetDistribution connection id: TrialSetDistribution =
  let sql = """
    select *
    from trial_set_distributions
    where id = @id
  """
  query
    connection
    sql 
    [intParam "id" id]
    toTrialSetDistribution

let insertSamplingDistribution connection samplingDistribution: SamplingDistribution = 
  let sql = """
    insert into sampling_distributions
    (
      trial_set_distribution_id, 
      sample_statistic, 
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
      @sampleStatistic,
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
  insertReturningId
    connection
    sql
    [
      intParam "trialSetDistributionId" samplingDistribution.trialSetDistributionId;
      stringParam "sampleStatistic" samplingDistribution.sampleStatistic;
      byteArrayParam "distribution" (DecimalList.encode 3 samplingDistribution.distribution);
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
  |> Option.map (fun id -> {samplingDistribution with id = id})


let computeAndStoreSamplingDistributions trialSetDistributionId: unit =
  let t1 = DateTime.Now
  
  let connection = ???
  let trialSetDistribution = lookupTrialSetDistribution connection trialSetDistributionId
  let sample = trialSetDistribution.distribution
  
  let numberOfSamples = 10000
  let numberOfObservationsPerSample = 10000
  // samplingDistributions = build_sampling_distributions(number_of_samples, number_of_observations_per_sample, build_1_year_return_sample_fn, sample_statistic_fns)
  let samplingDistributions = buildSamplingDistributionsFromOneMultiStatisticFn numberOfSamples numberOfObservationsPerSample build1YearReturnSample computeAllStats
  
  let t2 = DateTime.Now
  printfn "Building sampling distributions from %i samples, each containing %i observations, took %A seconds." numberOfSamples numberOfObservationsPerSample (t2 - t1)
  
  let returnPercentiles = samplingDistributions |> Array.map (Sample.Array.percentiles [| 1m; 10m; 20m; 30m; 40m; 50m; 60m; 70m; 80m; 90m; 99m |])
  printfn "%A" returnPercentiles
  
  let samplingDistributionMeans = samplingDistributions |> Array.map Sample.Array.mean
  printfn "%A" samplingDistributionMeans

  // compute sampling distributions
  ()

let run connectionString beanstalkdHost beanstalkdPort =
  info "Awaiting job from compute_missing_sampling_distributions queue"

  let dao = Postgres.createDao connectionString

  let client = Client.connect (beanstalkdHost, beanstalkdPort)
  client.watch "compute_missing_sampling_distributions" |> ignore

  let mutable keepLooping = true
  while keepLooping do
    let result = client.reserveWithTimeout 5
    match result with
    | Success (jobId, payload) ->
      printfn "jobId=%i  payload=%s" jobId payload

      let trialSetDistributionReference = decodeMessage payload

      computeAndStoreSamplingDistributions trialSetDistributionReference.trialSetDistributionId

      client.delete jobId |> ignore
    | jack.Failure msg ->
      failwith msg
      keepLooping <- false

