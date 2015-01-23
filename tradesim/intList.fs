module dke.tradesim.IntList

open System
open System.IO
open System.Numerics

type BitWriter() =
  let mutable pos = 0
  let mutable nextPos = 0
  let mutable bytesWritten = 0
  let mutable currentByte = 0
  let mutable stream = new MemoryStream()
  let mutable out = new BinaryWriter(stream)

  // <int> is treated as an unsigned integer/bit-string (i.e. no extra sign bit is written)
  member this.write (int: BigInteger) (n: int) =
    nextPos = pos + n

    while remainingBitsToWrite > 0 do
      val numberOfBitsToWrite = if (remainingBitsToWrite > numberOfFreeBitsInCurrentByte) {
        // then, we need to write to all of the remaining free bits in the current byte
        numberOfFreeBitsInCurrentByte
      } else {
        // write the remaining bits of <int> to a portion of the current byte
        remainingBitsToWrite
      }
      val rightmostBitsMask = (1 << numberOfBitsToWrite) - 1
      val rightShiftCount = remainingBitsToWrite - numberOfBitsToWrite
      currentByte = (currentByte << numberOfBitsToWrite) | ((int >> rightShiftCount).toInt & rightmostBitsMask)
      pos += numberOfBitsToWrite

      if (isAtBeginningOfByteBoundary) writeByte    // if we're at the beginning of a byte-boundary, we need to write the current byte to the bitstring and create a new byte-buffer

  member this.close() =
    if (!isAtBeginningOfByteBoundary) write(0, numberOfFreeBitsInCurrentByte)
    out.Close()

  member this.toByteArray(): array<byte> = stream.ToArray()

  let writeByte unit: unit =
    out.Write(currentByte)
    currentByte <- 0
    bytesWritten <- bytesWritten + 1

  let isAtBeginningOfByteBoundary = currentByteBitPosition == 0

  let currentByteBitPosition = pos % 8

  let numberOfFreeBitsInCurrentByte = 8 - currentByteBitPosition

  let remainingBitsToWrite = nextPos - pos


let decode (encodedInts: array<byte>): array<int> =
  [| |]

let encode (ints: array<int>): array<byte> =
  [| |]