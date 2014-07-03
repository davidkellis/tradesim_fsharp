module dke.tradesim.Securities

open Core
open Logging
open Database

let findExchanges (exchangeLabels: seq<string>) (dao: Dao<_>): seq<Exchange> =
  info (sprintf "findExchanges %s" <| String.join "," exchangeLabels)
  dao.findExchanges exchangeLabels

let Amex dao = findExchanges ["UA"] dao
let Nasdaq dao = findExchanges ["UW"; "UQ"; "UR"] dao
let Nyse dao = findExchanges ["UN"] dao
let PrimaryUsExchanges dao = Seq.apply [Amex; Nasdaq; Nyse] dao |> Seq.concat
let OTC_BB dao = findExchanges ["UU"] dao
let OTC dao = findExchanges ["UV"] dao

let findSecurities (exchanges: seq<Exchange>) (symbols: seq<string>) dao: seq<Security> =
  info (sprintf "findSecurities %s %s" (exchanges |> Seq.map (fun e -> e.label) |> String.join ",") (String.join "," symbols) )
  dao.findSecurities exchanges symbols
