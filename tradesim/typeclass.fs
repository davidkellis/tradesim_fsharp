module dke.tradesim.Typeclass

// typeclass
type AddSubtract<'t> = {
  add: 't -> 't -> 't
  subtract: 't -> 't -> 't
}

// user defined type
type Pair = int * int   // Pair is a 2-tuple

// instance of the type class defined on user defined type
let AddSubtractPair = {
  add = fun (a, b) (x, y) -> a + x, b + y
  subtract = fun (a, b) (x, y) -> a - x, b - y
}

let AddSubtractString = {
  add = fun (a: string) b -> a + b            // I'm defining add to do string concatenation
  subtract = fun a b -> a.Replace(b, "")      // I'm defining subtract to remove any occurance of b in a with ""
}

// helper functions to make it more natural to use functions defined in type class
let add tc a b = tc.add a b
let subtract tc a b = tc.subtract a b

let testAddSubtractTypeclass () =
  let a = (5, 6): Pair
  let b = (4, 10)

  let x = "abcdefg"
  let y = "cde"

  printfn "%A" a
  printfn "%A" b
  printfn "%A" (add AddSubtractPair a b)
  printfn "%A" (subtract AddSubtractPair a b)

  printfn "%A" x
  printfn "%A" y
  printfn "%A" (add AddSubtractString x y)
  printfn "%A" (subtract AddSubtractString x y)
