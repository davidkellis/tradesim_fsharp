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

type WeeklyTrialSetDefinition = {
  strategyName: string; 
  securityId: int;
  principal: decimal; 
  commissionPerTrade: decimal;
  commissionPerShare: decimal;
  mondayOfFirstWeeklyTrial: NodaTime.LocalDate;
  mondayOfLastWeeklyTrial: NodaTime.LocalDate
}

let decodeMessage message: WeeklyTrialSetDefinition =
  let json = JsonValue.Parse(message)
  let strategyName = json?strategy_name.AsString()
  let securityId = json?security_id.AsInteger()
  let principal = json?principal.AsDecimal()
  let commissionPerTrade = json?commission_per_trade.AsDecimal()
  let commissionPerShare = json?commission_per_share.AsDecimal()
  let mondayOfFirstWeeklyTrial = json?monday_of_first_weekly_trial.AsInteger() |> datestampToDate
  let mondayOfLastWeeklyTrial = json?monday_of_last_weekly_trial.AsInteger() |> datestampToDate
  {
    strategyName = strategyName 
    securityId = securityId
    principal = principal
    commissionPerTrade = commissionPerTrade
    commissionPerShare = commissionPerShare
    mondayOfFirstWeeklyTrial = mondayOfFirstWeeklyTrial
    mondayOfLastWeeklyTrial = mondayOfLastWeeklyTrial
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

      let trialSetDefinition = decodeMessage payload

      if trialSetDefinition.strategyName = strategies.BuyAndHold.StrategyName then
        strategies.BuyAndHold.Scenarios.runWeeklyTrials
          dao
          trialSetDefinition.securityId
          trialSetDefinition.principal
          trialSetDefinition.commissionPerTrade
          trialSetDefinition.commissionPerShare
          trialSetDefinition.mondayOfFirstWeeklyTrial
          trialSetDefinition.mondayOfLastWeeklyTrial

      client.delete jobId |> ignore
    | Failure msg ->
      failwith msg
      keepLooping <- false
