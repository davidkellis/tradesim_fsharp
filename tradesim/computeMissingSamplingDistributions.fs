module dke.returnStats.computeMissingSamplingDistributions

open System

open jack
open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharpx
open FSharpx.Collections
open NodaTime

open dke.tradesim
open Core
open Database
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

let decodeMessage message: TrialSetDistributionReference =
  let json = JsonValue.Parse(message)
  let trialSetDistributionId = json?trial_set_distribution_id.AsInteger()
  {
    trialSetDistributionId = trialSetDistributionId
  }

let computeAndStoreSamplingDistributions trialSetDistributionId: unit =
  let t1 = DateTime.Now
  
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

