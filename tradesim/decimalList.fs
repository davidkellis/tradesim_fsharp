module dke.tradesim.DecimalList

open System
open System.Numerics
open Math

// scalingFactor must be a non-negative integer
let decode (scalingFactor: int) (encodedDecimals: array<byte>): array<decimal> =
  let scalingDivider = Int.pow 10 scalingFactor |> decimal
  let integerEncodedDecimals = IntList.decode encodedDecimals
  integerEncodedDecimals |> Array.map (fun bigInt ->
    let d = decimal bigInt
    d / scalingDivider
  )

// scalingFactor must be a non-negative integer
let encode (scalingFactor: int) (decimals: array<decimal>): array<byte> =
  let scalingMultiplier = Int.pow 10 scalingFactor |> decimal
  let integerEncodedDecimals = decimals |> Array.map (fun d -> 
    let scaledDecimal = d * scalingMultiplier
    new BigInteger(scaledDecimal |> Decimal.round)
  )
  IntList.encode integerEncodedDecimals