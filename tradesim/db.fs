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


  //(strategy: Strategy<StateT>, trialStatePairs: seq<(Trial, StateT)>): Unit
//  insertTrials: TradingStrategy<'strategyT,'stateT> -> seq<Trial * 'stateT> -> unit

//  queryForTrial: (strategyName: string,
//                      securityId: SecurityId,
//                      trialDuration: Period,
//                      startDate: LocalDate,
//                      principal: BigDecimal,
//                      commissionPerTrade: BigDecimal,
//                      commissionPerShare: BigDecimal): Option<TrialsRow>

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


  //(strategy: Strategy<StateT>, trialStatePairs: seq<(Trial, StateT)>): Unit
//  insertTrials: TradingStrategy<'strategyT,'stateT> -> seq<Trial * 'stateT> -> unit

//  queryForTrial: (strategyName: string,
//                      securityId: SecurityId,
//                      trialDuration: Period,
//                      startDate: LocalDate,
//                      principal: BigDecimal,
//                      commissionPerTrade: BigDecimal,
//                      commissionPerShare: BigDecimal): Option<TrialsRow>

}


// query builder types and functions

type SqlValue =
  SqlInt of int
  | SqlLong of int64
  | SqlFloat of double
  | SqlString of string
  | SqlDecimal of decimal

type SqlParam =
  SqlPrimitive of string * SqlValue
  | SqlList of string * seq<SqlValue>

let escapeString (s: string): string = s.Replace("'", "''")

let sqlValueToSQL = function
  | SqlInt i -> sprintf "%i" i
  | SqlLong l -> sprintf "%i" l
  | SqlFloat f -> sprintf "%f" f
  | SqlString s -> sprintf "'%s'" (escapeString s)
  | SqlDecimal d -> sprintf "%M" d

let intParam name i = SqlPrimitive (name, SqlInt i)
let longParam name i = SqlPrimitive (name, SqlLong i)
let floatParam name f = SqlPrimitive (name, SqlFloat f)
let stringParam name s = SqlPrimitive (name, SqlString s)
let decimalParam name d = SqlPrimitive (name, SqlDecimal d)

let intListParam name values = SqlList (name, Seq.map SqlInt values)
let longListParam name values = SqlList (name, Seq.map SqlLong values)
let floatListParam name values = SqlList (name, Seq.map SqlFloat values)
let stringListParam name values = SqlList (name, Seq.map SqlString values)
let decimalListParam name values = SqlList (name, Seq.map SqlDecimal values)


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
       intListParam "exchangeIds" <| Seq.flatMap (fun (e: Exchange) -> e.id) exchanges]
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

  //let toTrial (reader: NpgsqlDataReader): Trial =
  //  {
  //    securityIds = [dbGetInt reader "security_id"]
  //    principal = dbGetDecimal reader "principal"
  //    commissionPerTrade = dbGetDecimal reader "commission_per_trade"
  //    commissionPerShare = dbGetDecimal reader "commission_per_share"
  //    startTime = dbGetLong reader "start_time" |> timestampToDatetime
  //    endTime = dbGetLong reader "end_time" |> timestampToDatetime
  //    duration = dbGetString reader "duration" |> ???
  //    incrementTime: ZonedDateTime -> ZonedDateTime
  //    purchaseFillPrice: PriceQuoteFn
  //    saleFillPrice: PriceQuoteFn
  //  }
  //
  //let queryForTrial
  //    (strategyName: String)
  //    (securityId: SecurityId)
  //    (trialDuration: Period)
  //    (startDate: LocalDate)
  //    (principal: decimal)
  //    (commissionPerTrade: decimal)
  //    (commissionPerShare: decimal) =
  //  let sql = """
  //    select s.id, trials.*
  //    from strategies strategy
  //    inner join trial_sets ts on ts.strategy_id = strategy.id
  //    inner join trials t on t.trial_set_id = ts.id
  //    inner join securities_trial_sets sts = sts on sts.trial_set_id = ts.id
  //    inner join securities s on s.id = sts.security_id
  //    where strategy.name = @strategyName
  //      and s.id = @securityId
  //      and ts.principal = @principal
  //      and ts.duration = @trialDuration
  //      and ts.commission_per_trade = @commissionPerTrade
  //      and ts.commission_per_share = @commissionPerShare
  //      and t.start_time >= @smallestStartTime
  //      and t.start_time <= @largestStartTime
  //    limit 1
  //  """
  //  query
  //    sql
  //    [
  //      stringParam "strategyName" strategyName;
  //      intParam "securityId" securityId;
  //      decimalParam "principal" principal;
  //      decimalParam "commissionPerTrade" commissionPerTrade;
  //      decimalParam "commissionPerShare" commissionPerShare;
  //      longParam "smallestStartTime" <| (localDateToDateTime startDate 0 0 0 |> dateTimeToTimestamp);
  //      longParam "largestStartTime" <| (localDateToDateTime startDate 23 59 59 |> dateTimeToTimestamp)
  //    ]
  //    toTrial
  //  >> Seq.firstOption

  // todo, implement trial select and insert queries


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
  }
