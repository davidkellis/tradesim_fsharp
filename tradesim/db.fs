module dke.tradesim.Database

open Core
open Time
open NodaTime
open System.Data.Linq
open System.Data.Entity
open Microsoft.FSharp.Data.TypeProviders

type EntityConnection = SqlEntityConnection<
                          Provider="Npgsql",
                          ConnectionString="Server=localhost;Port=5432;User Id=david;Password=;Database=tradesim;",
                          Pluralize = true>
let context = EntityConnection.GetDataContext()

//let queryEodBar (time: ZonedDateTime) (securityId: SecurityId): Bar option = 
//  let bars = EodBars.filter(_.securityId === securityId).filter(_.startTime <= timestamp(time))
//  let sortedBars = bars.sortBy(_.startTime.desc)
//  sortedBars.take(1).firstOption.map(convertEodBarsRow _)
