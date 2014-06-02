module dke.tradesim.Stdlib

let option (item: 't): Option<'t> = if item = null then None else Some item