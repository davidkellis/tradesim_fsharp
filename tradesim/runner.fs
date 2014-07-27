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
    [<Opt('d', "db", HelpText = "Database connection string")>]
    member val ConnectionString = "" with get, set

    [<Opt('u', "username", HelpText = "Database username")>]
    member val Username = "" with get, set

    [<Opt('p', "password", HelpText = "Database password")>]
    member val Password = "" with get, set

    [<Opt("build-trial-samples", HelpText = "Build missing trial samples")>]
    member val BuildTrialSamples = false with get, set

    [<Opt('s', "scenario", HelpText = "Run scenario <i>")>]
    member val Scenario = null with get, set

    [<HelpOption>]
    member this.GetUsage() = 
      HelpText.AutoBuild(
        this, 
        fun current -> HelpText.DefaultParsingErrorsHandler(this, current)
      ).ToString() +
      "Usage:\n\
        tradesim [--db jdbc:postgresql://localhost[:port]/tradesim] [--username <username>] [--password <password>] [--build-trial-samples | --scenario <scenarioName>]\n\n\
      Example:\n\
        tradesim --scenario buyandhold1\n"
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

  setLogLevel Verbose

  parsedOptions
  |> Option.map (fun options ->
    if options.BuildTrialSamples then
      info "build trial samples"
    elif options.Scenario <> null then
      info <| sprintf "run scenario %s" options.Scenario
      let connectionString = Postgres.buildConnectionString "localhost" 5432 "david" "" "tradesim"
      let dao = Postgres.createDao connectionString

      match options.Scenario with
      | "bah1" -> strategies.BuyAndHold.Scenarios.runSingleTrial1 dao
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
  |> ignore

  0
