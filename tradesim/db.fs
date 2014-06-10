module dke.tradesim.Database

open System
open System.Data
open System.Threading
open NodaTime
open Npgsql

open Stdlib
open Time
open Core
open Logging


type SqlValue =
  SqlInt of int
  | SqlFloat of double
  | SqlString of string

type SqlParam =
  SqlPrimitive of string * SqlValue
  | SqlList of string * seq<SqlValue>

// connection manipulation functions

let buildConnection host port username password database: NpgsqlConnection =
  let connectionString = sprintf "Server=%s;Port=%i;User Id=%s;Database=%s;" host port username database
  new NpgsqlConnection(connectionString)

let connect host port username password database: NpgsqlConnection =
  let connection = buildConnection host port username password database
  connection.Open()
  connection

let disconnect (connection: NpgsqlConnection) =
  connection.Close()


// helper for query function
//let buildSqlParameter (k: string, v: obj): NpgsqlParameter = NpgsqlParameter(k, v)

// returns the SQL that a NpgsqlCommand represents
// idea taken from http://stackoverflow.com/a/265261/458976
//let queryToString (cmd: NpgsqlCommand): string =
//  let replaceParamNamesWithParamValues = fun (queryStr: string) (p: NpgsqlParameter) -> queryStr.Replace(p.ParameterName, p.Value.ToString())
//  let parameters = cmd.Parameters.ToArray()
//  Seq.fold replaceParamNamesWithParamValues cmd.CommandText parameters

let sqlListParamKeyNames (key: string) (values: seq<'t>): seq<string> = 
  Seq.mapi (fun i value -> sprintf "@%s_%i" key i) values   // list of parameter names ["@<key>_0","@<key>_1", ...] that parallels <values>
 
//let addCommandParam (cmd: NpgsqlCommand) (p: SqlParam): NpgsqlCommand =
//  match p with
//  | SqlValue (key, value) ->
//    cmd.Parameters.AddWithValue(key, box value) |> ignore
//  | SqlList (key, values) ->
//    let keyNames = sqlListParamKeyNames key values
//    let keyValuePairs = Seq.zip keyNames values
//    Seq.iter 
//      (fun (paramName, paramValue) -> cmd.Parameters.AddWithValue(paramName, box paramValue) |> ignore) 
//      keyValuePairs
//  cmd

// replaces any SqlList parameters in a SQL fragment with an expanded parameter list
// idea taken from http://stackoverflow.com/questions/337704/parameterizing-an-sql-in-clause
// Example:
// expandQuery "a in (@lst1) or b in (@lst2)" [SqlList ("lst1", [box "a"; box "b"; box "c"]); SqlList ("lst2", [box 1; box 2; box 3])];;
// "a in (@lst1_0,@lst1_1,@lst1_2) or b in (@lst2_0,@lst2_1,@lst2_2)"
//let expandQuery (sql: string) (parameters: list<SqlParam>): string =
//  List.fold
//    (fun sql sqlParam -> 
//      match sqlParam with
//      | SqlList (key, values) ->
//        let keyNames = sqlListParamKeyNames key values
//        sql.Replace(sprintf "@%s" key, String.Join(",", keyNames))
//      | _ -> sql
//    )
//    sql
//    parameters


let escapeString (s: string): string = s.Replace("'", "''")

let sqlValueToSQL = function
  | SqlInt i -> sprintf "%i" i
  | SqlFloat f -> sprintf "%f" f
  | SqlString s -> sprintf "'%s'" (escapeString s)

// replaces parameters in a SQL fragment with the parameter value
// idea taken from http://stackoverflow.com/questions/337704/parameterizing-an-sql-in-clause
// Example:
// expandQuery "a in (@lst1) or b in (@lst2)" [SqlList ("lst1", [SqlString "a"; SqlString "b"; SqlString "c"]); SqlList ("lst2", [SqlInt 1; SqlInt 2; SqlInt 3])];;
// "a in ('a','b','c') or b in (1,2,3)"
let deparameterizeSql (sql: string) (parameters: list<SqlParam>): string =
  List.fold
    (fun sql sqlParam -> 
      match sqlParam with
      | SqlList (key, values) ->
        sql.Replace(sprintf "@%s" key, String.Join(",", Seq.map sqlValueToSQL values))
      | SqlPrimitive (key, value) ->
        sql.Replace(sprintf "@%s" key, sqlValueToSQL value)
    )
    sql
    parameters

let query (sql: string) (parameters: list<SqlParam>) (toType: NpgsqlDataReader -> 't) connection: seq<'t> = 
  seq {
    let deparameterizedSql = deparameterizeSql sql parameters
    let cmd = new NpgsqlCommand(deparameterizedSql, connection)
    cmd.CommandType <- CommandType.Text

//    let parameters = List.map buildSqlParameter parameterPairs
//    List.iter (fun p -> cmd.Parameters.Add(p) |> ignore) parameters
//    List.fold addCommandParam cmd parameters |> ignore

    verboseL <| lazy (str2 "SQL: " deparameterizedSql)

    let reader = cmd.ExecuteReader()
    while reader.Read() do
      yield reader |> toType
  }
 

// functions for building SQL queries

//let escapeString (str: string): string = str.Replace("'", "''")
//
//let quoteString (str: string): string = sprintf "'%s'" (escapeString str)
//
//let escapeList (alternatives: seq<string>): string = 
//  let quotedStrings = Seq.map quoteString alternatives
//  String.Join(",", quotedStrings)

let intParam name i = SqlPrimitive (name, SqlInt i)
let floatParam name f = SqlPrimitive (name, SqlFloat f)
let stringParam name s = SqlPrimitive (name, SqlString s)

let intListParam name values = SqlList (name, Seq.map SqlInt values)
let floatListParam name values = SqlList (name, Seq.map SqlFloat values)
let stringListParam name values = SqlList (name, Seq.map SqlString values)

let dbOpt<'t> (reader: NpgsqlDataReader) fieldName: Option<'t> = 
  if reader.IsDBNull(reader.GetOrdinal(fieldName)) then None else Some (unbox reader.[fieldName])


// database adapter functions

type DatabaseAdapter = {
  queryEodBar: ZonedDateTime -> SecurityId -> Bar option
}


// exchange queries

let toExchange (reader: NpgsqlDataReader): Exchange =
  { 
    id = dbOpt reader "id"
    label = unbox reader.["label"]
    name = dbOpt reader "name"
  }

let allExchanges = 
  let sql = "select * from exchanges"
  query sql [] toExchange

let findExchanges (labels: seq<string>) = 
  let sql = "select * from exchanges where label in (@labels)"
  query sql [stringListParam "labels" labels] toExchange


// security queries

let toSecurity (reader: NpgsqlDataReader): Security =
  { 
    id = dbOpt reader "id"
    bbGid = unbox reader.["bb_gid"]
    bbGcid = unbox reader.["bb_gcid"]
    kind = unbox reader.["type"]
    symbol = unbox reader.["symbol"]
    name = unbox reader.["name"]
    startDate = dbOpt reader "start_date"
    endDate = dbOpt reader "end_date"
    cik = dbOpt reader "cik"
    active = dbOpt reader "active"
    fiscalYearEndDate = dbOpt reader "fiscal_year_end_date"
    exchangeId = dbOpt reader "exchange_id"
    industryId = dbOpt reader "industry_id"
    sectorId = dbOpt reader "sector_id"
  }

let findSecurities (exchanges: seq<Exchange>) (symbols: seq<String>) = 
  let sql = """
    select s.id, s.bb_gid, s.bb_gcid, s.type, s.symbol, s.name, s.start_date, s.end_date, s.cik, s.active, s.fiscal_year_end_date, s.exchange_id, s.industry_id, s.sector_id
    from securities s
    inner join exchanges e on e.id = s.exchange_id
    where s.symbol in (@symbols)
      and e.id in (@exchangeIds)
  """
  query
    sql 
    [stringListParam "symbols" symbols; 
     intListParam "exchangeIds" <| Seq.flatMap (fun (e: Exchange) -> e.id) exchanges]
    toSecurity



//let queryEodBar (time: ZonedDateTime) (securityId: SecurityId): Bar option = 
//  let r = conn.Query<Bar>("select * from eod_bars where security_id = @securityId", {securityId = 123})
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
//