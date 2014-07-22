module dke.tradesim.Database

open System
open System.Data
open System.Threading
open FSharpx
open FSharpx.Collections
open NodaTime
open Npgsql

open Stdlib
open Time
open Core
open Logging
open Protobuf.FSharp


type DatabaseAdapter<'dbConnection> = {
  // (exchangeLabels: seq<string>): seq<Exchange>
  findExchanges: 'dbConnection -> seq<string> -> seq<Exchange>

  // (exchanges: seq<Exchange>, symbols: seq<string>): seq<Security>
  findSecurities: 'dbConnection -> seq<Exchange> -> seq<string> -> seq<Security>


  // (time: DateTime, securityId: SecurityId): Option<Bar>
  queryEodBar: 'dbConnection -> ZonedDateTime -> SecurityId -> Option<Bar>

  // (time: DateTime, securityId: SecurityId): Option<Bar>
  queryEodBarPriorTo: 'dbConnection -> ZonedDateTime -> SecurityId -> Option<Bar>

  // (securityId: SecurityId): seq<Bar>
  queryEodBars: 'dbConnection -> SecurityId -> seq<Bar>

  // (securityId: SecurityId, earliestTime: DateTime, latestTime: DateTime): seq<Bar>
  queryEodBarsBetween: 'dbConnection -> SecurityId -> ZonedDateTime -> ZonedDateTime -> seq<Bar>

  // (securityId: SecurityId): Option<Bar>
  findOldestEodBar: 'dbConnection -> SecurityId -> Option<Bar>

  // (securityId: SecurityId): Option<Bar>
  findMostRecentEodBar: 'dbConnection -> SecurityId -> Option<Bar>


  // (securityIds: IndexedSeq<int>): IndexedSeq<CorporateAction>
  queryCorporateActions: 'dbConnection -> seq<int> -> seq<CorporateAction>

  // (securityIds: IndexedSeq<int>, startTime: DateTime, endTime: DateTime): IndexedSeq<CorporateAction>
  queryCorporateActionsBetween: 'dbConnection -> seq<int> -> ZonedDateTime -> ZonedDateTime -> seq<CorporateAction>


  // (time: DateTime, securityId: SecurityId): Option<QuarterlyReport>
  queryQuarterlyReport: 'dbConnection -> ZonedDateTime -> SecurityId -> Option<QuarterlyReport>

  // (time: DateTime, securityId: SecurityId): Option<QuarterlyReport>
  queryQuarterlyReportPriorTo: 'dbConnection -> ZonedDateTime -> SecurityId -> Option<QuarterlyReport>

  // (securityId: SecurityId): seq<QuarterlyReport>
  queryQuarterlyReports: 'dbConnection -> SecurityId -> seq<QuarterlyReport>

  // (securityId: SecurityId, earliestTime: DateTime, latestTime: DateTime): seq<QuarterlyReport>
  queryQuarterlyReportsBetween: 'dbConnection -> SecurityId -> ZonedDateTime -> ZonedDateTime -> seq<QuarterlyReport>


  // (time: DateTime, securityId: SecurityId): Option<AnnualReport>
  queryAnnualReport: 'dbConnection -> ZonedDateTime -> SecurityId -> Option<AnnualReport>

  // (time: DateTime, securityId: SecurityId): Option<AnnualReport>
  queryAnnualReportPriorTo: 'dbConnection -> ZonedDateTime -> SecurityId -> Option<AnnualReport>

  // (securityId: SecurityId): seq<AnnualReport>
  queryAnnualReports: 'dbConnection -> SecurityId -> seq<AnnualReport>

  // (securityId: SecurityId, earliestTime: DateTime, latestTime: DateTime): seq<AnnualReport>
  queryAnnualReportsBetween: 'dbConnection -> SecurityId -> ZonedDateTime -> ZonedDateTime -> seq<AnnualReport>


  // (strategyName: string)
  // (securityId: SecurityId)
  // (trialDuration: Period)
  // (startDate: LocalDate)
  // (principal: BigDecimal)
  // (commissionPerTrade: BigDecimal)
  // (commissionPerShare: BigDecimal)
  // : Option<Trial>
  queryForTrial: 'dbConnection -> string -> SecurityId -> Period -> LocalDate -> decimal -> decimal -> decimal -> Option<Trial>

  // (strategyName: string) (trialStatePairs: seq<(Trial, BaseStrategyState)>): unit
  insertTrials: 'dbConnection -> string -> seq<Trial * BaseStrategyState> -> unit
}

type Dao<'dbConnection> = {
  // (exchangeLabels: seq<string>): seq<Exchange>
  findExchanges: seq<string> -> seq<Exchange>

  // (exchanges: seq<Exchange>, symbols: seq<string>): seq<Security>
  findSecurities: seq<Exchange> -> seq<string> -> seq<Security>


  // (time: DateTime, securityId: SecurityId): Option<Bar>
  queryEodBar: ZonedDateTime -> SecurityId -> Option<Bar>

  // (time: DateTime, securityId: SecurityId): Option<Bar>
  queryEodBarPriorTo: ZonedDateTime -> SecurityId -> Option<Bar>

  // (securityId: SecurityId): seq<Bar>
  queryEodBars: SecurityId -> seq<Bar>

  // (securityId: SecurityId, earliestTime: DateTime, latestTime: DateTime): seq<Bar>
  queryEodBarsBetween: SecurityId -> ZonedDateTime -> ZonedDateTime -> seq<Bar>

  // (securityId: SecurityId): Option<Bar>
  findOldestEodBar: SecurityId -> Option<Bar>

  // (securityId: SecurityId): Option<Bar>
  findMostRecentEodBar: SecurityId -> Option<Bar>


  // (securityIds: IndexedSeq<int>): IndexedSeq<CorporateAction>
  queryCorporateActions: seq<int> -> seq<CorporateAction>

  // (securityIds: IndexedSeq<int>, startTime: DateTime, endTime: DateTime): IndexedSeq<CorporateAction>
  queryCorporateActionsBetween: seq<int> -> ZonedDateTime -> ZonedDateTime -> seq<CorporateAction>


  // (time: DateTime, securityId: SecurityId): Option<QuarterlyReport>
  queryQuarterlyReport: ZonedDateTime -> SecurityId -> Option<QuarterlyReport>

  // (time: DateTime, securityId: SecurityId): Option<QuarterlyReport>
  queryQuarterlyReportPriorTo: ZonedDateTime -> SecurityId -> Option<QuarterlyReport>

  // (securityId: SecurityId): seq<QuarterlyReport>
  queryQuarterlyReports: SecurityId -> seq<QuarterlyReport>

  // (securityId: SecurityId, earliestTime: DateTime, latestTime: DateTime): seq<QuarterlyReport>
  queryQuarterlyReportsBetween: SecurityId -> ZonedDateTime -> ZonedDateTime -> seq<QuarterlyReport>


  // (time: DateTime, securityId: SecurityId): Option<AnnualReport>
  queryAnnualReport: ZonedDateTime -> SecurityId -> Option<AnnualReport>

  // (time: DateTime, securityId: SecurityId): Option<AnnualReport>
  queryAnnualReportPriorTo: ZonedDateTime -> SecurityId -> Option<AnnualReport>

  // (securityId: SecurityId): seq<AnnualReport>
  queryAnnualReports: SecurityId -> seq<AnnualReport>

  // (securityId: SecurityId, earliestTime: DateTime, latestTime: DateTime): seq<AnnualReport>
  queryAnnualReportsBetween: SecurityId -> ZonedDateTime -> ZonedDateTime -> seq<AnnualReport>


  // (strategyName: string)
  // (securityId: SecurityId)
  // (trialDuration: Period)
  // (startDate: LocalDate)
  // (principal: BigDecimal)
  // (commissionPerTrade: BigDecimal)
  // (commissionPerShare: BigDecimal)
  // : Option<TrialsRow>
  queryForTrial: string -> SecurityId -> Period -> LocalDate -> decimal -> decimal -> decimal -> Option<Trial>

  // (strategyName: string) (trialStatePairs: seq<(Trial, BaseStrategyState)>): unit
  insertTrials: string -> seq<Trial * BaseStrategyState> -> unit
}


// query builder types and functions

type SqlValue =
  SqlNull
  | SqlInt of int
  | SqlLong of int64
  | SqlFloat of double
  | SqlString of string
  | SqlDecimal of decimal
  | SqlByteArray of array<byte>

type SqlParam =
  SqlPrimitive of string * SqlValue
  | SqlList of string * seq<SqlValue>

let escapeString (s: string): string = s.Replace("'", "''")

let sqlValueToSQL = function
  | SqlNull -> "null"
  | SqlInt i -> sprintf "%i" i
  | SqlLong l -> sprintf "%i" l
  | SqlFloat f -> sprintf "%f" f
  | SqlString s -> sprintf "'%s'" (escapeString s)
  | SqlDecimal d -> sprintf "%M" d
  | SqlByteArray ba -> failwith "sqlValueToSQL doesn't work with byte arrays."

let intParam name i = SqlPrimitive (name, SqlInt i)
let longParam name i = SqlPrimitive (name, SqlLong i)
let floatParam name f = SqlPrimitive (name, SqlFloat f)
let stringParam name s = SqlPrimitive (name, SqlString s)
let decimalParam name d = SqlPrimitive (name, SqlDecimal d)
let byteArrayParam name ba = SqlPrimitive (name, SqlByteArray ba)

let optIntParam name i = i |> Option.map (fun i -> SqlPrimitive (name, SqlInt i)) |> Option.getOrElse (SqlPrimitive (name, SqlNull))
let optLongParam name i = i |> Option.map (fun i -> SqlPrimitive (name, SqlLong i)) |> Option.getOrElse (SqlPrimitive (name, SqlNull))
let optFloatParam name f = f |> Option.map (fun f -> SqlPrimitive (name, SqlFloat f)) |> Option.getOrElse (SqlPrimitive (name, SqlNull))
let optStringParam name s = s |> Option.map (fun s -> SqlPrimitive (name, SqlString s)) |> Option.getOrElse (SqlPrimitive (name, SqlNull))
let optDecimalParam name d = d |> Option.map (fun d -> SqlPrimitive (name, SqlDecimal d)) |> Option.getOrElse (SqlPrimitive (name, SqlNull))
let optByteArrayParam name ba = ba |> Option.map (fun ba -> SqlPrimitive (name, SqlByteArray ba)) |> Option.getOrElse (SqlPrimitive (name, SqlNull))

let intListParam name values = SqlList (name, Seq.map SqlInt values)
let longListParam name values = SqlList (name, Seq.map SqlLong values)
let floatListParam name values = SqlList (name, Seq.map SqlFloat values)
let stringListParam name values = SqlList (name, Seq.map SqlString values)
let decimalListParam name values = SqlList (name, Seq.map SqlDecimal values)
let byteArrayListParam name values = SqlList (name, Seq.map SqlByteArray values)


module Postgres = 

  // resultset helper functions
   
  let dbGet<'t> (reader: NpgsqlDataReader) (fieldName: string): 't = unbox<'t> reader.[fieldName]
  let dbGetInt = dbGet<int>
  let dbGetLong = dbGet<int64>
  let dbGetDecimal = dbGet<decimal>
  let dbGetStr = dbGet<string>
  let dbGetBool = dbGet<bool>
  let dbGetBytes (reader: NpgsqlDataReader) (fieldName: string): byte array = 
    let column = reader.GetOrdinal(fieldName)
    let len = reader.GetBytes(column, int64 0, null, 0, 0)      // get length of field
    let buffer: byte array = Array.zeroCreate (int32 len)       // create a buffer to hold the bytes, and then read the bytes from the DataTableReader
    reader.GetBytes(column, int64 0, buffer, 0, int32 len) |> ignore
    buffer

  let dbOpt<'t> (getterFn: NpgsqlDataReader -> string -> 't) (reader: NpgsqlDataReader) (fieldName: string): Option<'t> = 
    if reader.IsDBNull(reader.GetOrdinal(fieldName)) then None else Some (getterFn reader fieldName)
  let dbOptInt = dbOpt dbGetInt
  let dbOptLong: NpgsqlDataReader -> string -> Option<int64> = dbOpt dbGetLong
  let dbOptDecimal: NpgsqlDataReader -> string -> Option<decimal> = dbOpt dbGetDecimal
  let dbOptStr = dbOpt dbGetStr
  let dbOptBool = dbOpt dbGetBool
  let dbOptBytes: NpgsqlDataReader -> string -> Option<byte array> = dbOpt dbGetBytes


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

  let query connection (sql: string) (parameters: list<SqlParam>) (toType: NpgsqlDataReader -> 't): seq<'t> = 
    seq {
      let deparameterizedSql = deparameterizeSql sql parameters
      let cmd = new NpgsqlCommand(deparameterizedSql, connection)
      cmd.CommandType <- CommandType.Text

      verboseL <| lazy (str2 "SQL: " deparameterizedSql)

      let reader = cmd.ExecuteReader()
      while reader.Read() do
        yield reader |> toType
    }


  // data insertion functions

  let parameterizeSqlCommand (sqlCmd: NpgsqlCommand) (parameters: list<SqlParam>): NpgsqlCommand =
    List.iter
      (fun sqlParam -> 
        match sqlParam with
        | SqlList (key, values) ->
          let paramName = sprintf "@%s" key
          failwith "parameterizeSqlCommand doesn't implement parameterizing a query with a list."
        | SqlPrimitive (key, sqlValue) ->
          let paramName = sprintf "@%s" key
          (match sqlValue with
          | SqlInt value ->
            sqlCmd.Parameters.AddWithValue(paramName, NpgsqlTypes.NpgsqlDbType.Integer, value)
          | SqlLong value ->
            sqlCmd.Parameters.AddWithValue(paramName, NpgsqlTypes.NpgsqlDbType.Bigint, value)
          | SqlFloat value ->
            sqlCmd.Parameters.AddWithValue(paramName, NpgsqlTypes.NpgsqlDbType.Double, value)
          | SqlString value when value.Length <= 256 ->
            sqlCmd.Parameters.AddWithValue(paramName, NpgsqlTypes.NpgsqlDbType.Varchar, value)
          | SqlString value when value.Length > 256 ->
            sqlCmd.Parameters.AddWithValue(paramName, NpgsqlTypes.NpgsqlDbType.Text, value)
          | SqlDecimal value ->
            sqlCmd.Parameters.AddWithValue(paramName, NpgsqlTypes.NpgsqlDbType.Numeric, value)
          | SqlByteArray value ->
            sqlCmd.Parameters.AddWithValue(paramName, NpgsqlTypes.NpgsqlDbType.Bytea, value)
          | _ -> failwith "parameterizeSqlCommand failed: Unknown SqlPrimitive parameter type."
          ) |> ignore
      )
      parameters
    sqlCmd

  let insertReturningId connection (sql: string) (parameters: list<SqlParam>): Option<int> =
    let cmd = new NpgsqlCommand(sql, connection)
    cmd.CommandType <- CommandType.Text

    parameterizeSqlCommand cmd parameters |> ignore

    unboxedOpt (cmd.ExecuteScalar())

  // assumes the field name returned is "id"
  let insertReturningIds connection (sql: string) (parameters: list<SqlParam>): seq<int> =
    seq {
      let cmd = new NpgsqlCommand(sql, connection)
      cmd.CommandType <- CommandType.Text

      parameterizeSqlCommand cmd parameters |> ignore

      let reader = cmd.ExecuteReader()
      while reader.Read() do
        yield (dbGetInt reader "id")
    }


  let insert connection (sql: string) (parameters: list<SqlParam>): unit =
    let cmd = new NpgsqlCommand(sql, connection)
    cmd.CommandType <- CommandType.Text

    parameterizeSqlCommand cmd parameters |> ignore

    cmd.ExecuteNonQuery() |> ignore

  // exchange queries

  let toExchange (reader: NpgsqlDataReader): Exchange =
    { 
      id = dbOptInt reader "id"
      label = dbGetStr reader "label"
      name = dbOptStr reader "name"
    }

  let allExchanges connection: seq<Exchange> = 
    let sql = "select * from exchanges"
    query connection sql [] toExchange

  let findExchanges connection (labels: seq<string>): seq<Exchange> = 
    let sql = "select * from exchanges where label in (@labels)"
    query connection sql [stringListParam "labels" labels] toExchange


  // security queries

  let toSecurity (reader: NpgsqlDataReader): Security =
    { 
      id = dbOptInt reader "id"
      bbGid = dbGetStr reader "bb_gid"
      bbGcid = dbGetStr reader "bb_gcid"
      kind = dbGetStr reader "type"
      symbol = dbGetStr reader "symbol"
      name = dbGetStr reader "name"
      startDate = dbOptInt reader "start_date" |> Option.map datestampToDate
      endDate = dbOptInt reader "end_date" |> Option.map datestampToDate
      cik = dbOptInt reader "cik"
      active = dbOptBool reader "active"
      fiscalYearEndDate = dbOptInt reader "fiscal_year_end_date"
      exchangeId = dbOptInt reader "exchange_id"
      industryId = dbOptInt reader "industry_id"
      sectorId = dbOptInt reader "sector_id"
    }

  let findSecurities connection (exchanges: seq<Exchange>) (symbols: seq<String>): seq<Security> = 
    let sql = """
      select s.id, s.bb_gid, s.bb_gcid, s.type, s.symbol, s.name, s.start_date, s.end_date, s.cik, s.active, s.fiscal_year_end_date, s.exchange_id, s.industry_id, s.sector_id
      from securities s
      inner join exchanges e on e.id = s.exchange_id
      where s.symbol in (@symbols)
        and e.id in (@exchangeIds)
    """
    query
      connection
      sql 
      [stringListParam "symbols" symbols; 
       intListParam "exchangeIds" <| Seq.flatMapO (fun (e: Exchange) -> e.id) exchanges]
      toSecurity


  // EOD bar queries

  let toBar (reader: NpgsqlDataReader): Bar =
    {
      id = dbOptInt reader "id"
      securityId = dbGetInt reader "security_id"
      startTime = dbGetLong reader "start_time" |> timestampToDatetime
      endTime = dbGetLong reader "end_time" |> timestampToDatetime
      o = dbGetDecimal reader "open"
      h = dbGetDecimal reader "high"
      l = dbGetDecimal reader "low"
      c = dbGetDecimal reader "close"
      volume = dbGetLong reader "volume"
    }

  // returns the most recent fully-or-partially-observed EOD bar for the given security as of the given time
  let queryEodBar connection (time: ZonedDateTime) (securityId: SecurityId): Option<Bar> =
    let sql = """
      select * from eod_bars
      where security_id = @securityId and start_time <= @startTime
      order by start_time desc
      limit 1
    """
    query connection sql [intParam "securityId" securityId; longParam "startTime" <| dateTimeToTimestamp time] toBar |> Seq.firstOption

  // returns the most recent fully observed EOD bar for the given security as of the given time
  let queryEodBarPriorTo connection (time: ZonedDateTime) (securityId: SecurityId): Option<Bar> =
    let sql = """
      select * from eod_bars
      where security_id = @securityId and end_time < @endTime
      order by end_time desc
      limit 1
    """
    query connection sql [intParam "securityId" securityId; longParam "endTime" <| dateTimeToTimestamp time] toBar |> Seq.firstOption

  let queryEodBars connection (securityId: SecurityId): seq<Bar> =
    let sql = "select * from eod_bars where security_id = @securityId order by start_time"
    query connection sql [intParam "securityId" securityId] toBar

  let queryEodBarsBetween connection (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime): seq<Bar> =
    let sql = """
      select * from eod_bars 
      where security_id = @securityId
        and start_time >= @earliestTime
        and end_time <= @latestTime
      order by start_time
    """
    query
      connection
      sql
      [
        intParam "securityId" securityId;
        longParam "earliestTime" <| dateTimeToTimestamp earliestTime;
        longParam "latestTime" <| dateTimeToTimestamp latestTime
      ]
      toBar

  // returns the oldest EOD bar on record for the given security
  let findOldestEodBar connection (securityId: SecurityId): Option<Bar> =
    let sql = "select * from eod_bars where security_id = @securityId order by start_time"
    query connection sql [intParam "securityId" securityId] toBar |> Seq.firstOption

  // returns the newest (most recent) EOD bar on record for the given security
  let findMostRecentEodBar connection (securityId: SecurityId): Option<Bar> =
    let sql = "select * from eod_bars where security_id = @securityId order by start_time desc"
    query connection sql [intParam "securityId" securityId] toBar |> Seq.firstOption


  // corporate action (split/dividend) queries

  let toCorporateAction (reader: NpgsqlDataReader): CorporateAction =
    let kind = dbGetStr reader "type"
    match kind with
    | "Split" ->
      SplitCA {
        securityId = dbGetInt reader "security_id"
        exDate = dbGetInt reader "ex_date" |> datestampToDate
        ratio = dbGetDecimal reader "number"
      }
    | "CashDividend" ->
      CashDividendCA {
        securityId = dbGetInt reader "security_id"
        declarationDate = dbOptInt reader "declaration_date" |> Option.map datestampToDate  // date at which the announcement to shareholders/market that company will pay a dividend is made
        exDate = dbGetInt reader "ex_date" |> datestampToDate                               // on or after this date, the security trades without the dividend
        recordDate = dbOptInt reader "record_date" |> Option.map datestampToDate            // date at which shareholders of record are identified as recipients of the dividend
        payableDate = dbOptInt reader "payable_date" |> Option.map datestampToDate          // date at which company issues payment of dividend
        amount = dbGetDecimal reader "number"
      }
    | _ -> raise (new ArgumentException(sprintf "Unknown corporate action type: %s" kind))

  let queryCorporateActions connection (securityIds: seq<SecurityId>): seq<CorporateAction> =
    let sql = """
      select id, type, security_id, declaration_date, ex_date, record_date, payable_date, number from corporate_actions
      where security_id in (@securityIds)
      order by ex_date
    """
    query connection sql [intListParam "securityIds" securityIds] toCorporateAction

  let queryCorporateActionsBetween connection (securityIds: seq<SecurityId>) (startTime: ZonedDateTime) (endTime: ZonedDateTime): seq<CorporateAction> =
    let sql = """
      select id, type, security_id, declaration_date, ex_date, record_date, payable_date, number from corporate_actions
      where security_id in (@securityIds)
        and ex_date >= @startTime
        and ex_date <= @endTime
      order by ex_date
    """
    query
      connection
      sql
      [
        intListParam "securityIds" securityIds;
        longParam "startTime" <| dateTimeToTimestamp startTime;
        longParam "endTime" <| dateTimeToTimestamp endTime
      ]
      toCorporateAction


  // quarterly report queries

  let toStatement (binaryEncodedProtobufFinancialStatement: byte array): Statement = Map.empty      // todo, implement this

  let toQuarterlyReport (reader: NpgsqlDataReader): QuarterlyReport =
    {
      securityId = dbGetInt reader "security_id"
      startTime = dbGetLong reader "start_time" |> timestampToDatetime
      endTime = dbGetLong reader "end_time" |> timestampToDatetime
      publicationTime = dbGetLong reader "publication_time" |> timestampToDatetime
      incomeStatement = dbGetBytes reader "income_statement" |> toStatement
      balanceSheet = dbGetBytes reader "balance_sheet" |> toStatement
      cashFlowStatement = dbGetBytes reader "cash_flow_statement" |> toStatement
    }

  let queryQuarterlyReport connection (time: ZonedDateTime) (securityId: SecurityId): Option<QuarterlyReport> = 
    let sql = """
      select * from quarterly_reports
      where security_id = @securityId
        and start_time <= @startTime
      order by start_time desc
      limit 1
    """
    query
      connection
      sql
      [
        intParam "securityId" securityId;
        longParam "startTime" <| dateTimeToTimestamp time
      ]
      toQuarterlyReport
    |> Seq.firstOption

  let queryQuarterlyReportPriorTo connection (time: ZonedDateTime) (securityId: SecurityId): Option<QuarterlyReport> = 
    let sql = """
      select * from quarterly_reports
      where security_id = @securityId
        and end_time < @endTime
      order by end_time desc
      limit 1
    """
    query
      connection
      sql
      [
        intParam "securityId" securityId;
        longParam "endTime" <| dateTimeToTimestamp time
      ]
      toQuarterlyReport
    |> Seq.firstOption

  let queryQuarterlyReports connection (securityId: SecurityId): seq<QuarterlyReport> = 
    let sql = "select * from quarterly_reports where security_id = @securityId order by start_time"
    query connection sql [intParam "securityId" securityId] toQuarterlyReport

  let queryQuarterlyReportsBetween connection (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime): seq<QuarterlyReport> = 
    let sql = """
      select * from quarterly_reports 
      where security_id = @securityId 
        and start_time >= @earliestTime
        and end_time <= @latestTime
      order by start_time
    """
    query
      connection
      sql
      [
        intParam "securityId" securityId;
        longParam "earliestTime" <| dateTimeToTimestamp earliestTime;
        longParam "latestTime" <| dateTimeToTimestamp latestTime
      ]
      toQuarterlyReport


  // annual report queries

  let toAnnualReport (reader: NpgsqlDataReader): AnnualReport =
    {
      securityId = dbGetInt reader "security_id"
      startTime = dbGetLong reader "start_time" |> timestampToDatetime
      endTime = dbGetLong reader "end_time" |> timestampToDatetime
      publicationTime = dbGetLong reader "publication_time" |> timestampToDatetime
      incomeStatement = dbGetBytes reader "income_statement" |> toStatement
      balanceSheet = dbGetBytes reader "balance_sheet" |> toStatement
      cashFlowStatement = dbGetBytes reader "cash_flow_statement" |> toStatement
    }

  let queryAnnualReport connection (time: ZonedDateTime) (securityId: SecurityId): Option<AnnualReport> = 
    let sql = """
      select * from annual_reports
      where security_id = @securityId
        and start_time <= @startTime
      order by start_time desc
      limit 1
    """
    query
      connection
      sql
      [
        intParam "securityId" securityId;
        longParam "startTime" <| dateTimeToTimestamp time
      ]
      toAnnualReport
    |> Seq.firstOption

  let queryAnnualReportPriorTo connection (time: ZonedDateTime) (securityId: SecurityId): Option<AnnualReport> = 
    let sql = """
      select * from annual_reports
      where security_id = @securityId
        and end_time < @endTime
      order by end_time desc
      limit 1
    """
    query
      connection
      sql
      [
        intParam "securityId" securityId;
        longParam "endTime" <| dateTimeToTimestamp time
      ]
      toAnnualReport
    |> Seq.firstOption

  let queryAnnualReports connection (securityId: SecurityId): seq<AnnualReport> = 
    let sql = "select * from annual_reports where security_id = @securityId order by start_time"
    query connection sql [intParam "securityId" securityId] toAnnualReport

  let queryAnnualReportsBetween connection (securityId: SecurityId) (earliestTime: ZonedDateTime) (latestTime: ZonedDateTime): seq<AnnualReport> = 
    let sql = """
      select * from annual_reports 
      where security_id = @securityId 
        and start_time >= @earliestTime
        and end_time <= @latestTime
      order by start_time
    """
    query
      connection
      sql
      [
        intParam "securityId" securityId;
        longParam "earliestTime" <| dateTimeToTimestamp earliestTime;
        longParam "latestTime" <| dateTimeToTimestamp latestTime
      ]
      toAnnualReport


  // trial queries

  let toTrial (reader: NpgsqlDataReader): Trial =
    {
      securityIds = [dbGetInt reader "security_id"]
      principal = dbGetDecimal reader "principal"
      commissionPerTrade = dbGetDecimal reader "commission_per_trade"
      commissionPerShare = dbGetDecimal reader "commission_per_share"
      startTime = dbGetLong reader "start_time" |> timestampToDatetime
      endTime = dbGetLong reader "end_time" |> timestampToDatetime
      duration = dbGetStr reader "duration" |> parsePeriod |> Option.getOrElse (Period.FromSeconds(0L))

      // remaining attributes are irrelevant
      incrementTime = id
      purchaseFillPrice = fun t sId -> None
      saleFillPrice = fun t sId -> None
    }
  
  let queryForTrial
      connection
      (strategyName: String)
      (securityId: SecurityId)
      (trialDuration: Period)
      (startDate: LocalDate)
      (principal: decimal)
      (commissionPerTrade: decimal)
      (commissionPerShare: decimal)
      : Option<Trial> =
    let sql = """
      select s.id as security_id, t.*
      from strategies strategy
      inner join trial_sets ts on ts.strategy_id = strategy.id
      inner join trials t on t.trial_set_id = ts.id
      inner join securities_trial_sets sts = sts on sts.trial_set_id = ts.id
      inner join securities s on s.id = sts.security_id
      where strategy.name = @strategyName
        and s.id = @securityId
        and ts.principal = @principal
        and ts.duration = @trialDuration
        and ts.commission_per_trade = @commissionPerTrade
        and ts.commission_per_share = @commissionPerShare
        and t.start_time >= @smallestStartTime
        and t.start_time <= @largestStartTime
      limit 1
    """
    query
      connection
      sql
      [
        stringParam "strategyName" strategyName;
        intParam "securityId" securityId;
        decimalParam "principal" principal;
        decimalParam "commissionPerTrade" commissionPerTrade;
        decimalParam "commissionPerShare" commissionPerShare;
        longParam "smallestStartTime" <| (localDateToDateTime startDate 0 0 0 |> dateTimeToTimestamp);
        longParam "largestStartTime" <| (localDateToDateTime startDate 23 59 59 |> dateTimeToTimestamp)
      ]
      toTrial
    |> Seq.firstOption

  
  // strategy insertion functions

  type StrategyRecord = {id: int; name: string}

  let toStrategyRecord (reader: NpgsqlDataReader): StrategyRecord =
    {
      id = dbGetInt reader "id"
      name = dbGetStr reader "name"
    }

  let findStrategy connection (strategyName: string): Option<StrategyRecord> =
    let sql = """
      select * from securities
      where name = @name
      limit 1
    """
    query
      connection
      sql
      [stringParam "name" strategyName]
      toStrategyRecord
    |> Seq.firstOption

  let findOrCreateStrategy connection (strategyName: string): Option<StrategyRecord> =
    let insertRecord = fun unit ->
      let sql = """
        insert into strategies
        (name)
        values
        (@name)
        returning id;
      """
      insertReturningId
        connection
        sql
        [stringParam "name" strategyName]
      |> Option.map (fun id -> {id = id; name = strategyName})

    findStrategy connection strategyName |> Option.orElseF insertRecord


  // trialset insertion functions

  type TrialSetRecord = {
    id: int
    principal: decimal
    commissionPerTrade: decimal
    commissionPerShare: decimal
    duration: string
    strategyId: int
    securityIds: seq<int>
  }

  let toTrialSetRecord securityIds (reader: NpgsqlDataReader): TrialSetRecord =
    {
      id = dbGetInt reader "id"
      principal = dbGetDecimal reader "principal"
      commissionPerTrade = dbGetDecimal reader "commission_per_trade"
      commissionPerShare = dbGetDecimal reader "commission_per_share"
      duration = dbGetStr reader "duration"
      strategyId = dbGetInt reader "strategy_id"
      securityIds = securityIds
    }

  let toTrialSetIdSecurityIdPair (reader: NpgsqlDataReader): int * int =
    (dbGetInt reader "trial_set_id", dbGetInt reader "security_id")

  let findTrialSet connection (principal: decimal) (commissionPerTrade: decimal) (commissionPerShare: decimal) (duration: string) (strategyId: int) (securityIds: seq<SecurityId>): Option<TrialSetRecord> =
    let sql = """
      select
        ts.id as trial_set_id,
        s.id as security_id
      from trial_sets ts
      inner join securities_trial_sets sts on sts.trial_set_id = ts.id
      inner join securities s on sts.security_id = s.id
      where
        s.id in (@securityIds)
        and ts.principal = @principal
        and ts.commission_per_trade = @commissionPerTrade
        and ts.commission_per_share = @commissionPerShare
        and ts.duration = @duration
        and ts.strategy_id = @strategyId
      limit 1
    """
    let trialSetIdSecurityIdPairs = 
      query
        connection
        sql
        [
          intListParam "securityIds" securityIds
          decimalParam "principal" principal
          decimalParam "commissionPerTrade" commissionPerTrade
          decimalParam "commissionPerShare" commissionPerShare
          stringParam "duration" duration
          intParam "strategyId" strategyId
        ]
        toTrialSetIdSecurityIdPair

    let securityIdsGroupedByTrialSetId = 
      Seq.fold 
        (fun m (trialSetId, securityId) -> 
          let setOfSecurityIds = 
            Map.tryFind trialSetId m
            |> Option.getOrElse Set.empty
            |> Set.add securityId
          Map.add trialSetId setOfSecurityIds m
        ) 
        Map.empty
        trialSetIdSecurityIdPairs

    let desiredSecurityIdSet = Set.ofSeq securityIds

    let trialSetIdReferencingAllSecurities = Map.tryFindKey (fun trialSetId setOfSecurityIds -> setOfSecurityIds = desiredSecurityIdSet) securityIdsGroupedByTrialSetId

    trialSetIdReferencingAllSecurities
    |> Option.flatMap
      (fun trialSetId ->
        query connection "select * from trial_sets where id = @trialSetId" [intParam "trialSetId" trialSetId] (toTrialSetRecord securityIds) |> Seq.firstOption
      )

  let joinTrialSetToSecurities connection (securityIds: seq<SecurityId>) (trialSetId: int): unit =
    let sql = """
      insert into securities_trial_sets
      (trial_set_id, security_id)
      values
      (@trialSetId, @securityId);
    """
    Seq.iter
      (fun securityId ->
        insert
          connection
          sql
          [
            intParam "trialSetId" trialSetId
            intParam "securityId" securityId
          ]
      )
      securityIds

  let insertTrialSet connection (principal: decimal) (commissionPerTrade: decimal) (commissionPerShare: decimal) (duration: string) (strategyId: int) (securityIds: seq<SecurityId>): Option<TrialSetRecord> =
    let sql = """
      insert into trial_sets
      (principal, commission_per_trade, commission_per_share, duration, strategy_id)
      values
      (@principal, @commissionPerTrade, @commissionPerShare, @duration, @strategyId)
      returning id;
    """
    let trialSetId = insertReturningId
                       connection
                       sql
                       [
                         decimalParam "principal" principal
                         decimalParam "commissionPerTrade" commissionPerTrade
                         decimalParam "commissionPerShare" commissionPerShare
                         stringParam "duration" duration
                         intParam "strategyId" strategyId
                       ]

    trialSetId |> Option.iter (joinTrialSetToSecurities connection securityIds)

    Option.map
      (fun id ->
        {
          id = id
          principal = principal
          commissionPerTrade = commissionPerTrade
          commissionPerShare = commissionPerShare
          duration = duration
          strategyId = strategyId
          securityIds = securityIds
        }
      )
      trialSetId
  
  let findOrCreateTrialSet connection (strategyId: int) (trial: Trial): Option<TrialSetRecord> =
    let principal = trial.principal
    let commissionPerTrade = trial.commissionPerTrade
    let commissionPerShare = trial.commissionPerShare
    let duration = formatPeriod trial.duration
    let securityIds = trial.securityIds

    findTrialSet connection principal commissionPerTrade commissionPerShare duration strategyId securityIds
    |> Option.orElseF (fun unit -> insertTrialSet connection principal commissionPerTrade commissionPerShare duration strategyId securityIds)


  // trial insertion functions

  type TrialRecord = 
    {
      id: int
      startTime: int64
      endTime: int64
      transactionLog: protobuf.TransactionLog
      portfolioValueLog: protobuf.PortfolioValueLog
      trialYield: Option<decimal>
      mfe: Option<decimal>
      mae: Option<decimal>
      dailyStdDev: Option<decimal>
      trialSetId: int
    }

  let buildTrialRecord (trialId: int) (trialSetId: int) (trial: Trial) (state: BaseStrategyState): TrialRecord =
    {
      id = trialId
      startTime = dateTimeToTimestamp trial.startTime
      endTime = dateTimeToTimestamp trial.endTime
      transactionLog = convertTransactionsToProtobuf(state.transactions)
      portfolioValueLog = convertPortfolioValuesToProtobuf(state.portfolioValueHistory)
      trialYield = computeTrialYield trial state
      mfe = computeTrialMfe trial state
      mae = computeTrialMae trial state
      dailyStdDev = computeTrialStdDev state
      trialSetId = trialSetId
    }

  let insertTrialRecords connection (records: seq<TrialRecord>): unit =

    let valuesQueryFragment = 
      Seq.mapi
        (fun i trialRecord ->
          sprintf "(@startTime, @endTime, @transactionLog, @portfolioValueLog, @trialYield, @mfe, @mae, @dailyStdDev, @trialSetId)"
        )
        records

    let sql = sprintf
                """
                  insert into trials s
                  (start_time, end_time, transaction_log, portfolio_value_log, yield, mfe, mae, daily_std_dev, trial_set_id)
                  values
                  %s
                  returning id;
                """
                valuesQueryFragment

    insert
      connection
      sql
      [
        longParam "startTime" startTime
        longParam "endTime" endTime
        byteArrayParam "transactionLog" transactionLog
        byteArrayParam "portfolioValueLog" portfolioValueLog
        optDecimalParam "trialYield" trialYield
        optDecimalParam "mfe" mfe
        optDecimalParam "mae" mae
        optDecimalParam "dailyStdDev" dailyStdDev
        intParam "trialSetId" trialSetId
      ]
    |> ignore

  let insertTrials connection (strategyName: string) (trialStatePairs: seq<Trial * BaseStrategyState>): unit =
    Seq.tryHead trialStatePairs
    |> Option.iter
      (fun (firstTrial, firstStrategyState) ->
        let strategyRow = findOrCreateStrategy connection strategyName
        let trialSetRow = strategyRow |> Option.flatMap (fun (strategyRow: StrategyRecord) -> findOrCreateTrialSet connection strategyRow.id firstTrial)

        trialSetRow
        |> Option.iter
          (fun trialSetRow ->
            Seq.grouped 500 trialStatePairs
            |> Seq.iter
              (fun pairs ->
                verbose "Building group of records."

                pairs
                |> Seq.map (fun (trial, state) -> buildTrialRecord 0 trialSetRow.id trial state)
                |> insertTrialRecords connection
              )
          )
      )



  let Adapter: DatabaseAdapter<NpgsqlConnection> = {
    findExchanges = findExchanges
    findSecurities = findSecurities

    queryEodBar = queryEodBar
    queryEodBarPriorTo = queryEodBarPriorTo
    queryEodBars = queryEodBars
    queryEodBarsBetween = queryEodBarsBetween
    findOldestEodBar = findOldestEodBar
    findMostRecentEodBar = findMostRecentEodBar

    queryCorporateActions = queryCorporateActions
    queryCorporateActionsBetween = queryCorporateActionsBetween

    queryQuarterlyReport = queryQuarterlyReport
    queryQuarterlyReportPriorTo = queryQuarterlyReportPriorTo
    queryQuarterlyReports = queryQuarterlyReports
    queryQuarterlyReportsBetween = queryQuarterlyReportsBetween

    queryAnnualReport = queryAnnualReport
    queryAnnualReportPriorTo = queryAnnualReportPriorTo
    queryAnnualReports = queryAnnualReports
    queryAnnualReportsBetween = queryAnnualReportsBetween

    queryForTrial = queryForTrial
    insertTrials = insertTrials
  }

  let createDao connection: Dao<NpgsqlConnection> = {
    findExchanges = Adapter.findExchanges connection
    findSecurities = Adapter.findSecurities connection

    queryEodBar = Adapter.queryEodBar connection
    queryEodBarPriorTo = Adapter.queryEodBarPriorTo connection
    queryEodBars = Adapter.queryEodBars connection
    queryEodBarsBetween = Adapter.queryEodBarsBetween connection
    findOldestEodBar = Adapter.findOldestEodBar connection
    findMostRecentEodBar = Adapter.findMostRecentEodBar connection

    queryCorporateActions = Adapter.queryCorporateActions connection
    queryCorporateActionsBetween = Adapter.queryCorporateActionsBetween connection

    queryQuarterlyReport = Adapter.queryQuarterlyReport connection
    queryQuarterlyReportPriorTo = Adapter.queryQuarterlyReportPriorTo connection
    queryQuarterlyReports = Adapter.queryQuarterlyReports connection
    queryQuarterlyReportsBetween = Adapter.queryQuarterlyReportsBetween connection

    queryAnnualReport = Adapter.queryAnnualReport connection
    queryAnnualReportPriorTo = Adapter.queryAnnualReportPriorTo connection
    queryAnnualReports = Adapter.queryAnnualReports connection
    queryAnnualReportsBetween = Adapter.queryAnnualReportsBetween connection

    queryForTrial = Adapter.queryForTrial connection
    insertTrials = Adapter.insertTrials connection
  }
