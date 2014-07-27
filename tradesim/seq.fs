module dke.tradesim.Seq

open System.Collections.Immutable
open C5
open FSharpx.Collections

// Returns a lazy sequence of x, (f x), (f (f x)) etc.
// f must be free of side-effects
let iterate f x =
  let rec iterate' x = seq { yield x; yield! iterate' <| f x }
  iterate' x

// e.g. Seq.flatMapO (fun (e: Exchange) -> e.id) exchanges
let flatMapO (f: 't -> Option<'u>) (ts: seq<'t>): seq<'u> = 
  seq {
    for t in ts do
      let optU = f t
      match optU with 
      | Some u -> yield u
      | _ -> ()
  }

// e.g. Seq.flatMap (fun (e: Exchange) -> getSecurities e) exchanges
let flatMap (f: 't -> seq<'u>) (ts: seq<'t>): seq<'u> = 
  Seq.fold
    (fun memo t -> Seq.append memo (f t))
    Seq.empty
    ts

let firstOption (xs: seq<'t>): Option<'t> = Seq.tryPick Some xs

// assumes that the keys produced by running <entities> through <keyExtractorFn> are distinct
let treeMap<'k, 'v when 'k : comparison> (entities: seq<'v>) (keyExtractorFn: 'v -> 'k): TreeDictionary<'k, 'v> =
  let tree = new TreeDictionary<'k, 'v>()
  Seq.iter (fun value -> tree.Add(keyExtractorFn value, value)) entities
  tree

let mapIEnumerator (fn: 'x -> 'y) (xs: System.Collections.Generic.IEnumerator<'x>): seq<'y> =
  seq { 
    while xs.MoveNext() do
      yield fn xs.Current
  }

let fromIEnumerator (xs: System.Collections.Generic.IEnumerator<'x>): seq<'x> =
  seq { 
    while xs.MoveNext() do
      yield xs.Current
  }

let fromIEnumerableOfUnknownType<'x> (xs: System.Collections.IEnumerable): seq<'x> = Seq.cast xs

let fromIEnumerable (xs: System.Collections.Generic.IEnumerable<'x>): seq<'x> = fromIEnumerator (xs.GetEnumerator())

let groupIntoMapBy (fn: 't -> 'k) (ts: seq<'t>): Map<'k, seq<'t>> = Seq.groupBy fn ts |> Map.ofSeq

// Seq.apply [ (+) 1; (*) 2 ] 10 => [ 11; 20 ]
let apply (fns: seq<'t -> 'u>) (arg1: 't): seq<'u> = Seq.map (fun f -> f arg1) fns

let zipWithIndex (xs: seq<'t>): seq<'t * int> =
  Seq.initInfinite id
  |> Seq.zip xs

let reduceOption (reducerFn: 't -> 't -> 't) (xs: seq<'t>): Option<'t> =
  if Seq.isEmpty xs then
    None
  else
    Some <| Seq.reduce reducerFn xs

let grouped n (xs: seq<'t>): seq<seq<'t>> = 
  seq {
    let lst = ref <| new System.Collections.Generic.LinkedList<'t>()
    for x in xs do
      (!lst).AddLast(x) |> ignore

      if (!lst).Count % n = 0 then
        yield (!lst |> fromIEnumerable)
        lst := new System.Collections.Generic.LinkedList<'t>()

    if (!lst).Count % n <> 0 then
      yield ((!lst).GetEnumerator() |> fromIEnumerator)
  }