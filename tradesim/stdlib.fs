module dke.tradesim.Stdlib

open System

let option (item: 't): Option<'t> = if item = null then None else Some item

let unboxedOpt<'t> (item: obj): Option<'t> = if item = null then None else Some (unbox item)
