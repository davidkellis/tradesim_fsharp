﻿module dke.tradesim.Seq

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

let treeMap<'k, 'v when 'k : comparison> (entities: seq<'v>) (keyExtractorFn: 'v -> 'k): TreeDictionary<'k, 'v> =
  let tree = new TreeDictionary<'k, 'v>()
  Seq.iter (fun value -> tree.Add(keyExtractorFn value, value)) entities
  tree

let mapIEnumerator (fn: 'x -> 'y) (xs: System.Collections.Generic.IEnumerator<'x>): seq<'y> =
  seq { 
    while xs.MoveNext() do
      yield fn xs.Current
  }

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