module dke.tradesim.String

let join sep strings = String.concat sep strings

let joinInts sep (ints: seq<int>) = join sep <| Seq.map string ints

let substring (s: string) startIndex length = s.Substring(startIndex, length)

let replace (searchString: string) (replacementString: string) (s: string): string = s.Replace(searchString, replacementString)