module dke.tradesim.Seq

open C5

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

let treeMap<'k, 'v when 'k : comparison> (entities: seq<'v>) (keyExtractorFn: 'v -> 'k): TreeDictionary<'k, 'v> =
  let tree = new TreeDictionary<'k, 'v>()
  Seq.iter (fun value -> tree.Add(keyExtractorFn value, value)) entities
  tree


let iterIEnumerable (fn: 'x -> unit) (xs: System.Collections.Generic.IEnumerable<'x>): unit =
  for x in xs do
    fn x

let mapIEnumerator (fn: 'x -> 'y) (xs: System.Collections.Generic.IEnumerator<'x>): seq<'y> =
  seq { 
    while xs.MoveNext() do
      yield fn xs.Current
  }
