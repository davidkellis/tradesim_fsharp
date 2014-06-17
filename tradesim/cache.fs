module dke.tradesim.Cache

open CSharpTest.Net.Collections
//open Archer.Collections

// builds least-recently-used cache
let buildLruCache<'k, 'v when 'k : comparison> (maxEntries: int): LurchTable<'k, 'v> = 
  new LurchTable<'k, 'v>(LurchTableOrder.Access, maxEntries)

let get (cache: LurchTable<'k, 'v>) (key: 'k): Option<'v> = 
  match cache.TryGetValue(key) with
  | (true, value) -> Some value
  | _ -> None

let put (cache: LurchTable<'k, 'v>) (key: 'k) (value: 'v): unit = cache.Add(key, value)
