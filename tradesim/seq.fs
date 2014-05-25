module dke.tradesim.Seq

// Returns a lazy sequence of x, (f x), (f (f x)) etc.
// f must be free of side-effects
let iterate f x =
  let rec iterate' x = seq { yield x; yield! iterate' <| f x }
  iterate' x
