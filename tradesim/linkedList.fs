module dke.tradesim.LinkedList

open System

type LinkedList<'T> = Collections.Generic.LinkedList<'T>

module LinkedList =
  let empty<'T> (): LinkedList<'T> = new LinkedList<'T>()

  let isEmpty (list: LinkedList<'T>) = list.Count = 0   // LinkedList#Count is O(1)
  
  let isNotEmpty (list: LinkedList<'T>) = not (isEmpty list)
  
  // returns the new LinkedList node
  let addFirst (elem: 'T) (list: LinkedList<'T>): Collections.Generic.LinkedListNode<'T> = list.AddFirst(elem)

  let addFirstL (elem: 'T) (list: LinkedList<'T>): LinkedList<'T> = addFirst elem list |> ignore; list

  // returns the new LinkedList node
  let addLast (elem: 'T) (list: LinkedList<'T>): Collections.Generic.LinkedListNode<'T> = list.AddLast(elem)

  let addLastL (elem: 'T) (list: LinkedList<'T>): LinkedList<'T> = addLast elem list |> ignore; list
  
  let append (newElements: seq<'T>) (list: LinkedList<'T>): LinkedList<'T> =
    newElements |> Seq.iter (fun elem -> addLast elem list |> ignore)
    list

  // returns the first element from the list
  let first (list: LinkedList<'T>): Option<'T> =
    if isEmpty list then
      None
    else
      Some list.First.Value

  // returns the last element from the list
  let last (list: LinkedList<'T>): Option<'T> =
    if isEmpty list then
      None
    else
      Some list.Last.Value
  
  // removes and returns the first element from the list
  let removeFirst (list: LinkedList<'T>): Option<'T> =
    if isEmpty list then
      None
    else
      let node = list.First
      list.RemoveFirst()
      Some node.Value

  // removes and returns the last element from the list
  let removeLast (list: LinkedList<'T>): Option<'T> =
    if isEmpty list then
      None
    else
      let node = list.Last
      list.RemoveLast()
      Some node.Value

  let fromSeq (xs: seq<'T>): LinkedList<'T> =
    let list = empty<'T> ()
    xs |> Seq.iter (fun x -> addLast x list |> ignore)
    list
    
  let mapSeq (mappingFn: 'T -> 'U) (xs: seq<'T>): LinkedList<'U> =
    let list = empty<'U> ()
    xs |> Seq.iter (fun x -> addLast (mappingFn x) list |> ignore)
    list

  let toSeq (list: LinkedList<'T>): seq<'T> =
    Seq.fromIEnumerable list |> Seq.cache