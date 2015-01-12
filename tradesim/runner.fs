module dke.tradesim.Runner

open CommandLine
open CommandLine.Text

open System
open Database
open Logging

// command line parser example found at
// http://social.msdn.microsoft.com/Forums/en-US/9e858a99-19f4-437e-b157-57ec548f2e7b/using-commandline-library-from-f?forum=fsharpgeneral
type Opt = CommandLine.OptionAttribute
type RuntimeConfig() =
  class
    [<Opt('h', "host", HelpText = "Database host")>]
    member val Host = "localhost" with get, set

    [<Opt('p', "port", HelpText = "Database port")>]
    member val Port = 5432 with get, set

    [<Opt('d', "db", HelpText = "Database schema name")>]
    member val Database = "tradesim" with get, set

    [<Opt('u', "username", HelpText = "Database username")>]
    member val Username = "" with get, set

    [<Opt('p', "password", HelpText = "Database password")>]
    member val Password = "" with get, set

    [<Opt("qhost", HelpText = "Beanstalkd host")>]
    member val QHost = "localhost" with get, set

    [<Opt("qport", HelpText = "Beanstalkd port")>]
    member val QPort = 11300 with get, set

    [<Opt('q', "queue", HelpText = "Run commands received from Beanstalkd message queue")>]
    member val Queue = null with get, set

    [<Opt("build-trial-samples", HelpText = "Build missing trial samples")>]
    member val BuildTrialSamples = false with get, set

    [<Opt('s', "scenario", HelpText = "Run scenario <i>")>]
    member val Scenario = null with get, set

    [<Opt('v', "verbose", HelpText = "Turn on verbose logging")>]
    member val Verbose = false with get, set

    [<HelpOption>]
    member this.GetUsage() = 
      HelpText.AutoBuild(
        this, 
        fun current -> HelpText.DefaultParsingErrorsHandler(this, current)
      ).ToString() +
      "Usage:\n\
        In general:\n\
        tradesim [--verbose] [--host localhost] [--port 5432] [--db tradesim] [--username <username>] [--password <password>] [--qhost localhost] [--qport 11300] [--queue <beanstalkd queue name> | --build-trial-samples | --scenario <scenarioName>]\n\n\

        Run specific scenario\n\
        tradesim [--verbose] [--host localhost] [--port 5432] [--db tradesim] [--username <username>] [--password <password>] --scenario <scenarioName>\n\n\

        Build Trial Samples\n\
        tradesim [--verbose] [--host localhost] [--port 5432] [--db tradesim] [--username <username>] [--password <password>] --build-trial-samples\n\n\

        Listen to Beanstalkd queue for commands\n\
        tradesim [--verbose] [--host localhost] [--port 5432] [--db tradesim] [--username <username>] [--password <password>] [--qhost localhost] [--qport 11300] --queue <beanstalkd queue name>\n\n\

      Example:\n\
        tradesim --scenario bah1\n"
  end

let parseCommandLineArgs args =
  // command line parser example found at
  // http://social.msdn.microsoft.com/Forums/en-US/9e858a99-19f4-437e-b157-57ec548f2e7b/using-commandline-library-from-f?forum=fsharpgeneral
  let runtimeConfig = new RuntimeConfig()
  if Parser.Default.ParseArguments(args, runtimeConfig) then
    Some(runtimeConfig)
  else
    None


[<EntryPoint>]
let main argv = 
  let parsedOptions = parseCommandLineArgs argv

  parsedOptions
  |> Option.iter (fun options ->
    let connectionString = Postgres.buildConnectionString options.Host options.Port options.Username options.Password options.Database

    if options.Verbose then
      setLogLevel Verbose
    else
      setLogLevel Info

    if options.BuildTrialSamples then
      info "build trial samples"
      TrialSetStats.buildMissingTrialSetDistributions connectionString Core.TrialYield
    elif options.Queue <> null then
      info "Awaiting job from queue"

      let client = jack.Connection.parseAddress "localhost:11300" |> jack.Client.connect
      client.watch "tube_name_goes_here" |> ignore

      let mutable keepLooping = true
      while keepLooping do
        let result = client.reserveWithTimeout 5
        match result with
        | jack.Success (jobId, payload) ->
          printfn "jobId=%i  payload=%s" jobId payload
          client.delete jobId |> ignore
        | jack.Failure msg ->
          failwith msg
          keepLooping <- false

    elif options.Scenario <> null then
      info <| sprintf "run scenario %s" options.Scenario
      let dao = Postgres.createDao connectionString

      match options.Scenario with
      | "bah1" -> strategies.BuyAndHold.Scenarios.runSingleTrial1 dao
      | "bah2" -> strategies.BuyAndHold.Scenarios.runMultipleTrials1 dao
      | "bah3" -> strategies.BuyAndHold.Scenarios.compareMutualFunds dao
      | _ -> printfn "Unknown scenario"
      |> ignore

//      connection 
//      |> Postgres.allExchanges 
//      |> Seq.iter (fun e -> info <| sprintf "exchange: %A" e) 
//
//      info "********************************************************************"
//      dao.findExchanges <| Seq.ofList ["UQ"; "UA"]
//      |> Seq.iter (fun e -> info <| sprintf "exchange: %A" e) 
//
//      info "********************************************************************"
//      dao.findSecurities <| Postgres.allExchanges connection <| Seq.ofList ["AAPL"; "MSFT"]
//      |> Seq.iter (fun e -> info <| sprintf "security: %A" e)
    )

  0
