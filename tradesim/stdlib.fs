module dke.tradesim.Stdlib

open System

let composelr2 f g a1 a2 = g (f a1 a2)
let composelr3 f g a1 a2 a3 = g (f a1 a2 a3)
let composelr4 f g a1 a2 a3 a4 = g (f a1 a2 a3 a4)
let composelr5 f g a1 a2 a3 a4 a5 = g (f a1 a2 a3 a4 a5)

let opt (item: 't): Option<'t> = if item = null then None else Some item

let outParamToOpt (outParamGiven: bool, outParam: 'v): Option<'v> = if outParamGiven then Some outParam else None

let unboxedOpt<'t> (item: obj): Option<'t> = if item = null then None else Some (unbox item)

let replaceStr (searchString: String) (replacementStr: string) (origString: string): string = origString.Replace(searchString, replacementStr)

let str2 (a: string) (b: string): string =
  let builder = new System.Text.StringBuilder()
  builder.Append(a).Append(b).ToString()

let maxBy<'t, 'u when 'u : comparison> (valueFn: 't -> 'u) (a: 't) (b: 't): 't =
  if (valueFn a) >= (valueFn b) then a
  else b

let minBy<'t, 'u when 'u : comparison> (valueFn: 't -> 'u) (a: 't) (b: 't): 't =
  if (valueFn a) <= (valueFn b) then a
  else b

let to2ArgComparer<'t> (cmpFn: 't -> 't -> int): System.Collections.Generic.IComparer<'t> = 
  { 
    new System.Collections.Generic.IComparer<'t> 
    with member this.Compare(x, y) = cmpFn x y
  }

let toComparer<'t, 'k when 'k :> IComparable> (cmpFn: 't -> 'k): System.Collections.Generic.IComparer<'t> = 
  { 
    new System.Collections.Generic.IComparer<'t> 
    with member this.Compare(x: 't, y: 't) = (cmpFn x).CompareTo(cmpFn y)
  }