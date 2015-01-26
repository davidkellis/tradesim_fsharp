module dke.tradesim.DecimalList

let decode scalingFactor (encodedDecimals: array<byte>): array<decimal> =
  let integerEncodedDecimals = IntList.decodeInts encodedDecimals
  [| |]

let encode scalingFactor (decimals: array<decimal>): array<byte> =
  let integerEncodedDecimals = [| |]
  IntList.encodeInts integerEncodedDecimals