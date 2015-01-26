module dke.tradesim.ResizeArray

open System

module ResizeArray =
  let empty<'T> (): ResizeArray<'T> = new ResizeArray<'T>()

  let isEmpty (a: ResizeArray<'T>) = a.Count = 0

  let isNotEmpty (a: ResizeArray<'T>) = not (isEmpty a)

  let add (elem: 'T) (a: ResizeArray<'T>): ResizeArray<'T> = a.Add(elem); a

  let append (newElements: seq<'T>) (a: ResizeArray<'T>): ResizeArray<'T> = a.AddRange(newElements); a

  // returns the first element from the a
  let first (a: ResizeArray<'T>): Option<'T> =
    if isEmpty a then
      None
    else
      Some a.[0]

  // returns the last element from the a
  let last (a: ResizeArray<'T>): Option<'T> =
    if isEmpty a then
      None
    else
      Some a.[a.Count - 1]

  let fromSeq (xs: seq<'T>): ResizeArray<'T> = new ResizeArray<'T>(xs)

  let toArray (a: ResizeArray<'T>): array<'T> = a.ToArray()
    
  let map (mappingFn: 'T -> 'U) (xs: seq<'T>): ResizeArray<'U> = xs |> Seq.map mappingFn |> fromSeq
