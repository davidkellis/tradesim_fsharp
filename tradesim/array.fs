module dke.tradesim.Array

open Math

let getRandom (xs: 'T array): 'T =
  Array.get xs <| Random.randomInt xs.Length

let getRandomElements size (xs: array<'T>): array<'T> =
  Array.init size (fun _ -> getRandom xs)