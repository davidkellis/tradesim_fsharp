module dke.tradesim.DoubleList

open System
open System.Numerics
open Math

// scalingFactor must be a non-negative integer
let decode (scalingFactor: int) (encodedDoubles: array<byte>): array<double> =
  let scalingDivider = Int.pow 10 scalingFactor |> double
  let integerEncodedDoubles = IntList.decode encodedDoubles
  integerEncodedDoubles |> Array.map (fun bigInt ->
    let d = double bigInt
    d / scalingDivider
  )

// scalingFactor must be a non-negative integer
let encode (scalingFactor: int) (doubles: array<double>): array<byte> =
  let scalingMultiplier = Int.pow 10 scalingFactor |> double
  let integerEncodedDoubles = doubles |> Array.map (fun d -> 
    let scaledDouble = d * scalingMultiplier
    new BigInteger(scaledDouble |> Double.round)
  )
  IntList.encode integerEncodedDoubles