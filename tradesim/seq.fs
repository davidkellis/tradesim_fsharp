module dke.tradesim.Seq

// Returns a lazy sequence of x, (f x), (f (f x)) etc.
// f must be free of side-effects
let iterate f x =
  let rec iterate' x = seq { yield x; yield! iterate' <| f x }
  iterate' x

// e.g. Seq.flatMap (fun (e: Exchange) -> e.id) exchanges
let flatMap (f: 't -> Option<'u>) (ts: seq<'t>): seq<'u> = 
  Seq.fold
    (fun memo t -> 
      let optU = f t
      match optU with 
      | None -> memo 
      | Some u -> u :: memo)
    []
    ts
  |> List.toSeq

let firstOption (xs: seq<'t>): Option<'t> = Seq.tryPick Some xs