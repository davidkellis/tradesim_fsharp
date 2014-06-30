module dke.tradesim.Vector

open FSharpx
open FSharpx.Collections

// e.g. Vector.flatMapO (fun (e: Exchange) -> e.id) exchanges
let flatMapO (f: 't -> Option<'u>) (ts: Vector<'t>): Vector<'u> = 
  Vector.fold
    (fun memo t -> 
      let optU = f t
      match optU with 
      | None -> memo 
      | Some u -> Vector.conj u memo)
    Vector.empty
    ts

let concat v1 v2 =
  Vector.fold
    (fun vec elem -> Vector.conj elem vec)
    v1
    v2

// e.g. Vector.flatMap (fun (e: Exchange) -> getSecurities e) exchanges
let flatMap (f: 't -> Vector<'u>) (ts: Vector<'t>): Vector<'u> = 
  Vector.fold
    (fun memo t -> concat memo (f t))
    Vector.empty
    ts

let mapIEnumerator (fn: 'x -> 'y) (xs: System.Collections.Generic.IEnumerator<'x>): Vector<'y> =
  let rec mapIEnumeratorR (fn: 'x -> 'y) (xs: System.Collections.Generic.IEnumerator<'x>) ys =
    if xs.MoveNext() then
      mapIEnumeratorR fn xs (Vector.conj (fn xs.Current) ys)
    else
      ys
  mapIEnumeratorR fn xs Vector.empty

let mapSeq (f: 't -> 'u) (ts: seq<'t>): Vector<'u> = mapIEnumerator f <| ts.GetEnumerator()

let flatMapSeq (f: 't -> Vector<'u>) (ts: seq<'t>): Vector<'u> = 
  Seq.fold
    (fun memo t -> concat memo (f t))
    Vector.empty
    ts

let takeWhile (predicateFn: 'x -> bool) (xs: seq<'x>): Vector<'x> =
  let rec takeWhileR (xs: System.Collections.Generic.IEnumerator<'x>) memo: Vector<'x> =
    if xs.MoveNext() && (predicateFn xs.Current) then
      takeWhileR xs (Vector.conj xs.Current memo)
    else
      memo
  takeWhileR <| xs.GetEnumerator() <| Vector.empty

let groupIntoMapBy (fn: 't -> 'k) (ts: seq<'t>): Map<'k, seq<'t>> = ???