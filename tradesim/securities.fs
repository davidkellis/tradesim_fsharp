module dke.tradesim.Securities

open Core
open Logging
open Database

let findExchanges dao (exchangeLabels: seq<string>): seq<Exchange> =
  info (sprintf "findExchanges %s" <| String.join "," exchangeLabels)
  dao.findExchanges exchangeLabels

let Amex dao = findExchanges dao ["UA"]
let Nasdaq dao = findExchanges dao ["UW"; "UQ"; "UR"]
let Nyse dao = findExchanges dao ["UN"]
let PrimaryUsExchanges dao = Seq.apply [Amex; Nasdaq; Nyse] dao |> Seq.concat
let OTC_BB dao = findExchanges dao ["UU"]
let OTC dao = findExchanges dao ["UV"]

let findSecurities dao (exchanges: seq<Exchange>) (symbols: seq<string>): seq<Security> =
  info (sprintf "findSecurities %s %s" (exchanges |> Seq.map (fun e -> e.label) |> String.join ",") (String.join "," symbols) )
  dao.findSecurities exchanges symbols
