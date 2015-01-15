module dke.returnStats.runMissingWeeklyTrials

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
open strategies.BuyAndHold

type WeeklyTrialSetDefinition = {
  strategyName: string; 
  securityId: int;
  principal: decimal; 
  commissionPerTrade: decimal;
  commissionPerShare: decimal;
  intervalStart: NodaTime.LocalDate;
  intervalEnd: NodaTime.LocalDate
}

let decodeMessage message: WeeklyTrialSetDefinition =
  let json = JsonValue.Parse(message)
  let strategyName = json?strategy_name.AsString()
  let securityId = json?security_id.AsInteger()
  let principal = json?principal.AsDecimal()
  let commissionPerTrade = json?commission_per_trade.AsDecimal()
  let commissionPerShare = json?commission_per_share.AsDecimal()
  let intervalStart = json?interval_start.AsInteger() |> datestampToDate
  let intervalEnd = json?interval_end.AsInteger() |> datestampToDate
  {
    strategyName = strategyName 
    securityId = securityId
    principal = principal
    commissionPerTrade = commissionPerTrade
    commissionPerShare = commissionPerShare
    intervalStart = intervalStart
    intervalEnd = intervalEnd
  }

// returns weekly non-overlapping intervals
let buildNonOverlappingWeeklyTrialIntervalsBetween startDate endDate: seq<Interval> =
  let oneWeek = weeks 1L
  let firstMondayAtMarketOpen = firstMondayAtOrAfter startDate |> localDateToDateTime 9 30 0
  let lastMondayAtMarketOpen = nthWeekdayAtOrBeforeDate 2 DayOfWeek.Monday endDate |> localDateToDateTime 9 30 0
  interspersedIntervals2 firstMondayAtMarketOpen lastMondayAtMarketOpen oneWeek oneWeek

let runWeeklyTrials dao securityId principal commissionPerTrade commissionPerShare intervalStart intervalEnd: unit =
  let timeIncrementerFn = defaultOneDayTimeIncrementer
  let purchaseFillPriceFn = tradingBloxFillPriceWithSlippage dao (findEodBar dao) barSimQuote barHigh 0.3M
  let saleFillPriceFn = tradingBloxFillPriceWithSlippage dao (findEodBar dao) barSimQuote barLow 0.3M
  let strategy = buildStrategy dao
  let trialGenerator = buildTrialGenerator principal commissionPerTrade commissionPerShare timeIncrementerFn purchaseFillPriceFn saleFillPriceFn
  let securityIds = [securityId] |> Vector.ofSeq
  let trialIntervals = buildNonOverlappingWeeklyTrialIntervalsBetween intervalStart intervalEnd
  let trialPeriodLength = weeks 1L

  info <| sprintf "Building trials for %i" securityId
  let trials = buildTrialsOverIntervals trialIntervals trialGenerator securityIds trialPeriodLength
  info <| sprintf "Running %i trials" (Seq.length trials)
  let (finalStates, trialResults) = runAndLogTrialsInParallel TradingStrategyImpl StrategyStateImpl dao strategy trials
  TrialSetStats.printReport trialResults


let run connectionString beanstalkdHost beanstalkdPort =
  info "Awaiting job from run_missing_weekly_fund_trials queue"

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
        runWeeklyTrials
          dao
          trialSetDefinition.securityId
          trialSetDefinition.principal
          trialSetDefinition.commissionPerTrade
          trialSetDefinition.commissionPerShare
          trialSetDefinition.intervalStart
          trialSetDefinition.intervalEnd

      client.delete jobId |> ignore
    | jack.Failure msg ->
      failwith msg
      keepLooping <- false
