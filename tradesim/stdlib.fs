module dke.tradesim.Stdlib

open System

let opt (item: 't): Option<'t> = if item = null then None else Some item

let unboxedOpt<'t> (item: obj): Option<'t> = if item = null then None else Some (unbox item)

let replaceStr (searchString: String) (replacementStr: string) (origString: string): string = origString.Replace(searchString, replacementStr)

let str2 (a: string) (b: string): string =
  let builder = new System.Text.StringBuilder()
  builder.Append(a).Append(b).ToString()