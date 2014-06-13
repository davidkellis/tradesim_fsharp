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


type DatabaseAdapter = {
  queryEodBar: ZonedDateTime -> SecurityId -> Bar option
}


// query builder types and functions

type SqlValue =
  SqlInt of int
  | SqlLong of int64
  | SqlFloat of double
  | SqlString of string

type SqlParam =
  SqlPrimitive of string * SqlValue
  | SqlList of string * seq<SqlValue>

let escapeString (s: string): string = s.Replace("'", "''")

let sqlValueToSQL = function
  | SqlInt i -> sprintf "%i" i
  | SqlLong l -> sprintf "%i" l
  | SqlFloat f -> sprintf "%f" f
  | SqlString s -> sprintf "'%s'" (escapeString s)

let intParam name i = SqlPrimitive (name, SqlInt i)
let longParam name i = SqlPrimitive (name, SqlLong i)
let floatParam name f = SqlPrimitive (name, SqlFloat f)
let stringParam name s = SqlPrimitive (name, SqlString s)

let intListParam name values = SqlList (name, Seq.map SqlInt values)
let longListParam name values = SqlList (name, Seq.map SqlLong values)
let floatListParam name values = SqlList (name, Seq.map SqlFloat values)
let stringListParam name values = SqlList (name, Seq.map SqlString values)


// resultset helper functions
 
let dbOpt<'t> (reader: NpgsqlDataReader) fieldName: Option<'t> = 
  if reader.IsDBNull(reader.GetOrdinal(fieldName)) then None else Some (unbox reader.[fieldName])


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


// query execution functions

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

    verboseL <| lazy (str2 "SQL: " deparameterizedSql)

    let reader = cmd.ExecuteReader()
    while reader.Read() do
      yield reader |> toType
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


// EOD bar queries

let toBar (reader: NpgsqlDataReader): Bar =
  {
    id = dbOpt reader "id"
    securityId = unbox reader.["security_id"]
    startTime = timestampToDatetime <| unbox reader.["start_time"]
    endTime = timestampToDatetime <| unbox reader.["end_time"]
    o = unbox reader.["open"]
    h = unbox reader.["high"]
    l = unbox reader.["low"]
    c = unbox reader.["close"]
    volume = unbox reader.["volume"]
  }

// returns the most recent fully-or-partially-observed EOD bar for the given security as of the given time
let queryEodBar (time: ZonedDateTime) (securityId: SecurityId) =
  let sql = """
    select * from eod_bars
    where security_id = @securityId and start_time <= @startTime
    order by start_time desc
    limit 1
  """
  query sql [intParam "securityId" securityId; longParam "startTime" <| dateTimeToTimestamp time] toBar >> Seq.firstOption

// returns the most recent fully observed EOD bar for the given security as of the given time
let queryEodBarPriorTo (time: ZonedDateTime) (securityId: SecurityId) =
  let sql = """
    select * from eod_bars
    where security_id = @securityId and end_time < @endTime
    order by end_time desc
    limit 1
  """
  query sql [intParam "securityId" securityId; longParam "endTime" <| dateTimeToTimestamp time] toBar >> Seq.firstOption

let queryEodBars (securityId: SecurityId) =
  let sql = "select * from eod_bars where security_id = @securityId order by start_time"
  query sql [intParam "securityId" securityId] toBar

let queryEodBarsBetween (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime) =
  let sql = """
    select * from eod_bars 
    where security_id = @securityId
      and start_time >= @earliestTime
      and end_time <= @latestTime
    order by start_time
  """
  query
    sql
    [
      intParam "securityId" securityId;
      longParam "earliestTime" <| dateTimeToTimestamp earliestTime;
      longParam "latestTime" <| dateTimeToTimestamp latestTime
    ]
    toBar

// returns the oldest EOD bar on record for the given security
let findOldestEodBar (securityId: SecurityId) =
  let sql = "select * from eod_bars where security_id = @securityId order by start_time"
  query sql [intParam "securityId" securityId] toBar >> Seq.firstOption

// returns the newest (most recent) EOD bar on record for the given security
let findMostRecentEodBar (securityId: SecurityId) =
  let sql = "select * from eod_bars where security_id = @securityId order by start_time desc"
  query sql [intParam "securityId" securityId] toBar >> Seq.firstOption


// corporate action (split/dividend) queries

let toCorporateAction (reader: NpgsqlDataReader): CorporateAction =
  let kind = unbox reader.["type"]
  match kind with
  | "Split" ->
    SplitCA {
      securityId = unbox reader.["security_id"]
      exDate = datestampToDate <| unbox reader.["ex_date"]
      ratio = unbox reader.["number"]
    }
  | "CashDividend" ->
    CashDividendCA {
      securityId = unbox reader.["security_id"]
      declarationDate = Option.map datestampToDate <| dbOpt reader "declaration_date"  // date at which the announcement to shareholders/market that company will pay a dividend is made
      exDate = datestampToDate <| unbox reader.["ex_date"]                          // on or after this date, the security trades without the dividend
      recordDate = Option.map datestampToDate <| dbOpt reader "record_date"            // date at which shareholders of record are identified as recipients of the dividend
      payableDate = Option.map datestampToDate <| dbOpt reader "payable_date"          // date at which company issues payment of dividend
      amount = unbox reader.["number"]
    }
  | _ -> raise (new ArgumentException(sprintf "Unknown corporate action type: %s" kind))

let queryCorporateActions (securityIds: seq<SecurityId>) =
  let sql = """
    select id, type, security_id, declaration_date, ex_date, record_date, payable_date, number from corporate_actions
    where security_id in (@securityIds)
    order by ex_date
  """
  query sql [intListParam "securityIds" securityIds] toCorporateAction

let queryCorporateActionsBetween (securityIds: seq<SecurityId>) (startTime: ZonedDateTime) (endTime: ZonedDateTime) =
  let sql = """
    select id, type, security_id, declaration_date, ex_date, record_date, payable_date, number from corporate_actions
    where security_id in (@securityIds)
      and ex_date >= @startTime
      and ex_date <= @endTime
    order by ex_date
  """
  query
    sql
    [
      intListParam "securityIds" securityIds;
      longParam "startTime" <| dateTimeToTimestamp startTime;
      longParam "endTime" <| dateTimeToTimestamp endTime
    ]
    toCorporateAction


// quarterly report queries

let toQuarterlyReport (reader: NpgsqlDataReader): CorporateAction =
  let kind = unbox reader.["type"]
  match kind with
  | "Split" ->
    SplitCA {
      securityId = unbox reader.["security_id"]
      exDate = datestampToDate <| unbox reader.["ex_date"]
      ratio = unbox reader.["number"]
    }
  | "CashDividend" ->
    CashDividendCA {
      securityId = unbox reader.["security_id"]
      declarationDate = Option.map datestampToDate <| dbOpt reader "declaration_date"  // date at which the announcement to shareholders/market that company will pay a dividend is made
      exDate = datestampToDate <| unbox reader.["ex_date"]                          // on or after this date, the security trades without the dividend
      recordDate = Option.map datestampToDate <| dbOpt reader "record_date"            // date at which shareholders of record are identified as recipients of the dividend
      payableDate = Option.map datestampToDate <| dbOpt reader "payable_date"          // date at which company issues payment of dividend
      amount = unbox reader.["number"]
    }
  | _ -> raise (new ArgumentException(sprintf "Unknown corporate action type: %s" kind))
