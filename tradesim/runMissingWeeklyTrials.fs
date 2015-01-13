module dke.returnStats.runMissingWeeklyTrials

open System

open jack
open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharpx

open dke.tradesim
open Database
open Logging
open Time

type TrialDefinition = {
  strategyName: string; 
  principal: decimal; 
  commissionPerTrade: decimal;
  commissionPerShare: decimal;
  duration: NodaTime.Period;
  startTime: NodaTime.ZonedDateTime;
  strategyParams: Map<string, string>;
  securityId: int
}

let decodeMessage message: TrialDefinition =
  let json = JsonValue.Parse(message)
  let strategyName = json?strategyName.AsString()
  let principal = json?principal.AsDecimal()
  let commissionPerTrade = json?commission_per_trade.AsDecimal()
  let commissionPerShare = json?commission_per_share.AsDecimal()
  let duration = json?duration.AsString()
  let startTime = json?start_time.AsInteger64() |> timestampToDatetime
  let strategyParams = 
    (json?strategy_params).Properties 
    |> Array.map (function (k,v) -> (k, v.AsString()))
    |> Map.ofArray
  let securityId = json?security_id.AsInteger()
  {
    strategyName = strategyName 
    principal = principal
    commissionPerTrade = commissionPerTrade
    commissionPerShare = commissionPerShare
    duration = duration |> parsePeriod |> Option.getOrElse (NodaTime.Period.FromWeeks(1L))
    startTime = startTime
    strategyParams = strategyParams
    securityId = securityId
  }

let run connectionString beanstalkdHost beanstalkdPort =
  info "Awaiting job from queue run_missing_weekly_fund_trials"

  let dao = Postgres.createDao connectionString

  let client = Client.connect (beanstalkdHost, beanstalkdPort)
  client.watch "run_missing_weekly_fund_trials" |> ignore

  let mutable keepLooping = true
  while keepLooping do
    let result = client.reserveWithTimeout 5
    match result with
    | Success (jobId, payload) ->
      printfn "jobId=%i  payload=%s" jobId payload

      let trialDefinition = decodeMessage payload

      if trialDefinition.strategyName = strategies.BuyAndHold.StrategyName then
        strategies.BuyAndHold.Scenarios.runWeeklyTrial 
          dao
          trialDefinition.principal
          trialDefinition.commissionPerTrade
          trialDefinition.commissionPerShare
          trialDefinition.duration
          trialDefinition.startTime
          trialDefinition.securityId

      client.delete jobId |> ignore
    | Failure msg ->
      failwith msg
      keepLooping <- false
