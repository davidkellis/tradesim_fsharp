module dke.tradesim.BitManipulation

open System
open System.Numerics
open FSharpx

open LinkedList
open Range

module BigInteger =
  let Int32MaxValueAsBigInteger = BigInteger 0x7FFFFFFF
  let toInt (bigInt: BigInteger): int =
    let signMask = bigInt.Sign &&& 0x80000000
    let valueMask = int (bigInt &&& Int32MaxValueAsBigInteger)
    signMask ||| valueMask

  // ithLeastSignificantBit is a 0-based index from the right-most (least-significant) bit of the byte
  let getBit (bigInt: BigInteger) (ithLeastSignificantBit: int): int = int (bigInt >>> ithLeastSignificantBit) &&& 1

  // Per http://docs.oracle.com/javase/7/docs/api/java/math/BigInteger.html#bitLength():
  // Returns the number of bits in the minimal two's-complement representation of this BigInteger, excluding a sign bit. 
  // For positive BigIntegers, this is equivalent to the number of bits in the ordinary binary representation. 
  // Computes ceil(log2(this < 0 ? -this : this+1))
  let bitLength (i: BigInteger): int =
    let value = if i < BigInteger.Zero then -i else i + BigInteger.One
    BigInteger.Log(value, 2.0) |> Math.Ceiling |> int

  // returns the 0-based index position of the most-significant-bit with respect to the least-significant-bit (i.e. the least-significant-bit is as index 0)
  let mostSignificantBitPosition (i: BigInteger): int =
    let len = bitLength i
    if len = 0 then 0 else len - 1

  // mostSignificantBit(5I) => 1
  // mostSignificantBit(0I) => 0
  let mostSignificantBit (i: BigInteger) (mostSignificantBitIndex: Option<int>): int = 
    let index = mostSignificantBitIndex |> Option.getOrElse (mostSignificantBitPosition i)
    getBit i index

  let flipBit (i: BigInteger) (index: int): BigInteger = i ^^^ (1I <<< index)
    
  let uToS (unsignedInt: BigInteger) (mostSignificantBitIndex: int): BigInteger =
    let msb = mostSignificantBit unsignedInt <| Some mostSignificantBitIndex
    if msb = 1 then
      -(BigInteger(1) <<< mostSignificantBitIndex) + flipBit unsignedInt mostSignificantBitIndex
    else unsignedInt

  (*
   * deltaEncodeInts(List(50, 60, 70, 75))
   * res14: Seq[int] = List(50, 10, 10, 5)
   *)
  let deltaEncodeInts (ints: seq<BigInteger>): seq<BigInteger> =
    let rec deltaEncodeIntsR (ints: LinkedList<BigInteger>) (prev: BigInteger) (newList: LinkedList<BigInteger>): seq<BigInteger> =
      match ints |> LinkedList.removeFirst with
      | None -> newList |> LinkedList.toSeq
      | Some next -> deltaEncodeIntsR ints next (newList |> LinkedList.addLastL (next - prev))

    deltaEncodeIntsR <| LinkedList.fromSeq ints <| 0I <| LinkedList.empty ()
  
  (*
   * deltaDecodeInts(List(50, 10, 10, 5))
   * res13: Seq[int] = List(50, 60, 70, 75)
   *)
  let deltaDecodeInts (ints: seq<BigInteger>): seq<BigInteger> =
    ints
    |> Seq.fold 
      (fun (list, sum) i -> 
        let newSum = sum + i
        (LinkedList.addLastL newSum list, newSum)
      )
      (LinkedList.empty (), 0I)
    |> fst
    |> LinkedList.toSeq


module Byte =
  let getBit (b: byte) (ithLeastSignificantBit: int): int = int (b >>> ithLeastSignificantBit) &&& 1

  let mostSignificantBit (b: byte): int = getBit b 7

  // extractInt(0b11101010, 6, 2)
  // => 26
  // extractInt(0b11101010, 5, 2)
  // => 10
  let extractInt (b: byte) (msbit: int) (lsbit: int): int =
    if msbit < lsbit then failwith "most-significant-bit position cannot be less than the least-significant-bit position"
    if lsbit < 0 || msbit > 7 then failwith "least-significant-bit position must be >= 0 and most-significant-bit position must be < 8"
    (lsbit, msbit) |> Range.foldInclusive (fun sum ithLeastSignificantBit -> sum + getBit b ithLeastSignificantBit * (1 <<< (ithLeastSignificantBit - lsbit)) ) 0

  // extractIntLR(0b11101010, 0, 2)
  // => 7
  // extractIntLR(0b11101010, 1, 3)
  // => 6
  // extractIntLR(0b11101010, 5, 7)
  // => 2
  let extractIntLR (b: byte) (leftIndex: int) (rightIndex: int): int =
    if leftIndex > rightIndex then failwith "leftIndex cannot be greater than the rightIndex"
    extractInt b (7 - leftIndex) (7 - rightIndex)


module Int =
  let getBit (i: int) (ithLeastSignificantBit: int): int = int (i >>> ithLeastSignificantBit) &&& 1

  let mostSignificantBit (i: int): int = getBit i 31

  let bitLength (i: int): int = BigInteger.bitLength <| BigInteger(i)

module Int64 =
  let getBit (i64: int64) (ithLeastSignificantBit: int): int = int (i64 >>> ithLeastSignificantBit) &&& 1

  let mostSignificantBit (i: int64): int = getBit i 63

  let bitLength (i: int64): int = BigInteger.bitLength <| BigInteger(i)






//  let bitLengthInts(ints: Seq[int]): int = ints.foldLeft(0){ (sum, i) => sum + bitLength(i) }
//  let bitLengthint64s(ints: Seq[int64]): int = ints.foldLeft(0){ (sum, i) => sum + bitLength(i) }
//  let bitLengthBigIntegers(ints: Seq[BigInteger]): int = ints.foldLeft(0){ (sum, i) => sum + bitLength(i) }
//
//  let printBits(int: byte) { 7.to(0).by(-1).foreach(i => print(getBit(int, i))) }
//  let printBits(int: int) { 31.to(0).by(-1).foreach(i => print(getBit(int, i))) }
//  let printBits(int: int64) { 63.to(0).by(-1).foreach(i => print(getBit(int, i))) }
//  let printBits(int: BigInteger) { 128.to(0).by(-1).foreach(i => print(getBit(int, i))) }
//  let printBits(ints: Array[byte]) { ints.foreach{i => printBits(i); print(" ")} }
//  let printBits(ints: Seq[BigInteger]) { ints.foreach{i => printBits(i); print(" ")} }
//
