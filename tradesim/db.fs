module dke.tradesim.Database

open System.Data
open System.Threading
open NodaTime
open Npgsql

open Stdlib
open Core
open Time

let buildConnection host port username password database: NpgsqlConnection =
  let connectionString = sprintf "Server=%s;Port=%i;User Id=%s;Database=%s;" host port username database
  new NpgsqlConnection(connectionString)

let connect host port username password database: NpgsqlConnection =
  let connection = buildConnection host port username password database
  connection.Open()
  connection

let disconnect (connection: NpgsqlConnection) =
  connection.Close()

let param name value = (name, box value)

let buildSqlParameter (k: string, v: obj) = NpgsqlParameter(k, v)

let query connection toType (sql: string) (parameterPairs: list<string * obj>) = 
  seq {
    let cmd = new NpgsqlCommand(sql, connection)
    cmd.CommandType <- CommandType.Text

    let parameters = List.map buildSqlParameter parameterPairs
    List.iter (fun p -> cmd.Parameters.Add(p) |> ignore) parameters

    let reader = cmd.ExecuteReader()
    while reader.Read() do
      yield reader |> toType
  }

let dbOpt<'t> (reader: NpgsqlDataReader) fieldName: Option<'t> = 
  if reader.IsDBNull(reader.GetOrdinal(fieldName)) then None else Some (unbox reader.[fieldName])

let toExchange (reader: NpgsqlDataReader): Exchange =
  { 
    id = dbOpt reader "id"
    label = unbox reader.["label"]
    name = dbOpt reader "name"
  }

let allExchanges: seq<Exchange> = 
  let conn = connect "localhost" 5432 "david" "" "tradesim"
  query conn toExchange "select * from exchanges" []

//let queryEodBar (time: ZonedDateTime) (securityId: SecurityId): Bar option = 
//  let conn = connect "a" 1 "u" "p" "db"
//  let r = conn.Query<Bar>("select * from eod_bars where security_id = @securityId", {"securityId" = 123})
//  Some {
//    securityId = 4
//    startTime = datetime 1 2 3 4 5 6
//    endTime = datetime 1 2 3 4 5 6
//    o = 1.2M
//    h = 1.4M
//    l = 1.1M
//    c = 1.3M
//    volume = 10L
//  }
                
//  let bars = EodBars.filter(_.securityId === securityId).filter(_.startTime <= timestamp(time))
//  let sortedBars = bars.sortBy(_.startTime.desc)
//  sortedBars.take(1).firstOption.map(convertEodBarsRow _)
