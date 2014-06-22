module dke.tradesim.String

let join sep strings = String.concat sep strings

let joinInts sep (ints: seq<int>) = join sep <| Seq.map string ints