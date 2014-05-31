module dke.tradesim.Database

open NodaTime
open System.Data.Linq
open Npgsql
open Dapper

open Core
open Time

let connect host port username password database: NpgsqlConnection =
  let connectionString = sprintf "Server=%s;Port=%i;User Id=%s;Password=%s;Database=%s;" host port username password database
  let connection = new NpgsqlConnection(connectionString)
  connection.Open()
  connection

let disconnect (connection: NpgsqlConnection) =
  connection.Close()

let queryEodBar (time: ZonedDateTime) (securityId: SecurityId): Bar option = 
  let conn = connect "a" 1 "u" "p" "db"
  let r = conn.Query<Bar>("select * from eod_bars where security_id = :securityId", [("securityId", 123)] |> Map.ofList)
  Some {
    securityId = 4
    startTime = datetime 1 2 3 4 5 6
    endTime = datetime 1 2 3 4 5 6
    o = 1.2M
    h = 1.4M
    l = 1.1M
    c = 1.3M
  }
                
//  let bars = EodBars.filter(_.securityId === securityId).filter(_.startTime <= timestamp(time))
//  let sortedBars = bars.sortBy(_.startTime.desc)
//  sortedBars.take(1).firstOption.map(convertEodBarsRow _)
