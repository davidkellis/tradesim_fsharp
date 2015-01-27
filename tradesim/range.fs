module dke.tradesim.Range

open System.Threading.Tasks

type Range = int * int

let iter (fn: int -> unit) (range: Range): unit =
  let (startI, endI) = range
  for i = startI to endI do
    fn i

// fold over the range [start, end)  (i.e. excluding the end)
let fold (accFn: 'State -> int -> 'State) (state: 'State) (range: Range) =
  let rec foldR state i endI =
    if i >= endI then
      state
    else
      foldR (accFn state i) (i + 1) endI

  let (startI, endI) = range
  foldR state startI endI

// fold over the range [start, end]  (i.e. including the end)
let foldInclusive (accFn: 'State -> int -> 'State) (state: 'State) (range: Range) =
  let rec foldR state i endI =
    if i > endI then
      state
    else
      foldR (accFn state i) (i + 1) endI

  let (startI, endI) = range
  foldR state startI endI

let map (fn: int -> 'T) (range: Range): array<'T> =
  let (startI, endI) = range
  let length = endI - startI + 1
  if length < 0 then
    failwith "The start of the range must not exceed the end of the range."
  else
    Array.init length (fun i -> fn (i + startI))


(* Parallel versions *)

let piter (fn: int -> unit) (range: Range): unit =
  let (startI, endI) = range
  Parallel.For(startI, endI, (fun i -> fn i)) |> ignore

let pmap (fn: int -> 'T) (range: Range): array<'T> =
  let (startI, endI) = range
  let length = endI - startI + 1
  if length < 0 then
    failwith "The start of the range must not exceed the end of the range."
  else
    Array.Parallel.init length (fun i -> fn (i + startI))

