module dke.tradesim.IntList

open System
open System.IO
open System.Numerics

open BitManipulation
open ResizeArray
open LinkedList

type BitWriter() =
  let mutable pos = 0
  let mutable nextPos = 0
  let mutable bytesWritten = 0
  let mutable currentByte = 0uy
  let stream = new MemoryStream()
  let out = new BinaryWriter(stream)

  member private this.writeByte(): unit =
    out.Write(currentByte)
    currentByte <- 0uy
    bytesWritten <- bytesWritten + 1

  member private this.isAtBeginningOfByteBoundary = this.currentByteBitPosition = 0

  member private this.currentByteBitPosition = pos % 8

  member private this.numberOfFreeBitsInCurrentByte = 8 - this.currentByteBitPosition

  member private this.remainingBitsToWrite = nextPos - pos

  // <int> is treated as an unsigned integer/bit-string (i.e. no extra sign bit is written)
  member this.write (i: BigInteger) (n: int) =
    nextPos <- pos + n

    while this.remainingBitsToWrite > 0 do
      let numberOfBitsToWrite = 
        if this.remainingBitsToWrite > this.numberOfFreeBitsInCurrentByte then
          // then, we need to write to all of the remaining free bits in the current byte
          this.numberOfFreeBitsInCurrentByte
        else
          // write the remaining bits of <int> to a portion of the current byte
          this.remainingBitsToWrite

      let rightmostBitsMask = (1 <<< numberOfBitsToWrite) - 1
      let rightShiftCount = this.remainingBitsToWrite - numberOfBitsToWrite
      currentByte <- (currentByte <<< numberOfBitsToWrite) ||| byte (BigInteger.toInt (i >>> rightShiftCount) &&& rightmostBitsMask)
      pos <- pos + numberOfBitsToWrite

      if (this.isAtBeginningOfByteBoundary) then
        this.writeByte()    // if we're at the beginning of a byte-boundary, we need to write the current byte to the bitstring and create a new byte-buffer

  member this.close() =
    if not this.isAtBeginningOfByteBoundary then
      this.write 0I this.numberOfFreeBitsInCurrentByte
    out.Close()

  member this.toByteArray(): array<byte> = stream.ToArray()


type BitReader(byteSeq: MemoryStream) =
  let mutable pos = 0
  let mutable nextPos = 0
  let mutable bytesRead = 0
  let mutable currentByte = 0uy

  member private this.readByte(): byte =
    let b = byteSeq.ReadByte()
    if b = -1 then failwith "End of stream has been reached."
    bytesRead <- bytesRead + 1
    byte b

  member private this.isAtBeginningOfByteBoundary = this.currentByteBitPosition = 0

  member private this.currentByteBitPosition = pos % 8

  member private this.numberOfUnreadBitsInCurrentByte = 8 - this.currentByteBitPosition

  member private this.remainingBitsToRead = nextPos - pos

  // returns an unsigned integer representing the <n> bits read from the bit-stream
  member this.read (n: int): BigInteger =
    let mutable sum = 0I
    if n > 0 then
      nextPos <- pos + n
      while this.remainingBitsToRead > 0 do
        if this.isAtBeginningOfByteBoundary then
          currentByte <- this.readByte()      // if we're at the beginning of a byte-boundary, we need to read the "current byte" into memory
        
        let numberOfBitsToRead = 
          if this.remainingBitsToRead > this.numberOfUnreadBitsInCurrentByte then
            // then, read all the unread bits in the current byte
            this.numberOfUnreadBitsInCurrentByte
          else
            // read just a portion of the current byte
            this.remainingBitsToRead
        
        sum <- (sum <<< numberOfBitsToRead) ||| BigInteger(Byte.extractIntLR currentByte this.currentByteBitPosition (this.currentByteBitPosition + numberOfBitsToRead - 1))
        pos <- pos + numberOfBitsToRead
    sum


module VariableByteSignedIntEncoder =
  (*
   * bw is a BitWriter object
   * int is an unsigned integer
   * returns the number of bytes written to the BitWriter
   *)
  let write (bw: BitWriter) (i: BigInteger): int =
    let bitCount = BigInteger.bitLength i
    let bitCountMod7 = bitCount % 7
    let evenlyDivisibleBitCount = if bitCountMod7 = 0 then bitCount else bitCount + (7 - bitCountMod7)    // number of bits required to hold <i> in a whole number of 7-bit words
    let sevenBitWordCount = evenlyDivisibleBitCount / 7
    let mutable tmpInt = 0

    for wordIndex = 1 to (sevenBitWordCount - 1) do
      let shiftAmount = (sevenBitWordCount - wordIndex) * 7
      tmpInt <- (BigInteger.toInt (i >>> shiftAmount) &&& 0x7F) ||| 0x80
      bw.write (BigInteger tmpInt) 8

    tmpInt <- BigInteger.toInt i &&& 0x7F
    bw.write (BigInteger tmpInt) 8

    sevenBitWordCount

  (*
   * bw is a BitWriter object
   * i is a signed integer
   * returns the number of bytes written to the BitWriter
   *)
  let writeSigned (bw: BitWriter) (i: BigInteger): int =
    let bitCount = BigInteger.bitLength i + 1   // + 1 to account for the sign bit we prefix the integer with
    let bitCountMod7 = bitCount % 7
    let evenlyDivisibleBitCount = if bitCountMod7 = 0 then bitCount else bitCount + (7 - bitCountMod7)    // number of bits required to hold <i> in a whole number of 7-bit words
    let sevenBitWordCount = evenlyDivisibleBitCount / 7
    let mutable tmpInt = 0
    
    for wordIndex = 1 to (sevenBitWordCount - 1) do
      let shiftAmount = (sevenBitWordCount - wordIndex) * 7
      tmpInt <- (BigInteger.toInt (i >>> shiftAmount) &&& 0x7F) ||| 0x80
      bw.write (BigInteger tmpInt) 8
    
    tmpInt <- BigInteger.toInt i &&& 0x7F
    bw.write (BigInteger tmpInt) 8

    sevenBitWordCount

  // returns an unsigned int read from BitReader object <br>
  let read (br: BitReader): BigInteger =
    let mutable i = br.read 8
    let mutable sum = i &&& (BigInteger 0x7F)
    while BigInteger.mostSignificantBit i <| Some 7 = 1 do
      i <- br.read 8
      sum <- (sum <<< 7) ||| (i &&& BigInteger(0x7F))
    sum

  // returns a signed int read from BitReader object <br>
  let readSigned (br: BitReader): BigInteger =
    let mutable i = br.read 8
    let mutable count = 1
    let mutable sum = i &&& (BigInteger 0x7F)
    while BigInteger.mostSignificantBit i <| Some 7 = 1 do
      i <- br.read 8
      count <- count + 1
      sum <- (sum <<< 7) ||| (i &&& BigInteger(0x7F))
    BigInteger.uToS sum (7 * count - 1)


module FrameOfReferenceIntListEncoder =
  let write (bw: BitWriter) (ints: array<BigInteger>): unit =
    let numberOfInts = ints.Length
    if numberOfInts > 0 then
      let signedMin = ints |> Array.min
      let offsets = ints |> Array.map (fun i -> i - signedMin)   // all offsets are guaranteed to be non-negative
      let maxOffset = offsets |> Array.max
      let intBitSize = BigInteger.bitLength maxOffset

      VariableByteSignedIntEncoder.writeSigned bw signedMin |> ignore
      VariableByteSignedIntEncoder.write bw <| BigInteger numberOfInts |> ignore
      VariableByteSignedIntEncoder.write bw <| BigInteger intBitSize |> ignore
      
      offsets |> Array.iter (fun i -> bw.write i intBitSize)

  let read (br: BitReader): array<BigInteger> =
    let signedMinInt = VariableByteSignedIntEncoder.readSigned br
    let numberOfInts = VariableByteSignedIntEncoder.read br |> int
    let intBitSize = VariableByteSignedIntEncoder.read br |> int

    let ints = ResizeArray.empty<BigInteger> ()
    let mutable i = 0
    while i < numberOfInts do
      ResizeArray.add (signedMinInt + br.read intBitSize) ints |> ignore
      i <- i + 1
    ints |> ResizeArray.toArray


type BinaryPackingIntListEncoder(blockSizeInBits: int) =
  new() =
    BinaryPackingIntListEncoder(128)

  member this.write (bw: BitWriter) (ints: seq<BigInteger>): unit =
    if not (Seq.isEmpty ints) then
      let sortedInts = Seq.sort ints
      let deltaEncodedInts = BigInteger.deltaEncodeInts sortedInts

      let signedStartInt = deltaEncodedInts |> Seq.head
      let remainingInts = deltaEncodedInts |> Seq.tail
      let slices = remainingInts |> Seq.slice blockSizeInBits |> Seq.toArray
      let numberOfSlices = slices.Length

      VariableByteSignedIntEncoder.writeSigned bw signedStartInt |> ignore
      VariableByteSignedIntEncoder.write bw <| BigInteger numberOfSlices |> ignore

      slices |> Array.iter (fun sliceOfInts -> FrameOfReferenceIntListEncoder.write bw <| Seq.toArray sliceOfInts)

  member this.read (br: BitReader): seq<BigInteger> =
    let signedStartInt = VariableByteSignedIntEncoder.readSigned br
    let numberOfSlices = VariableByteSignedIntEncoder.read br |> int

    let mutable deltaEncodedIntList = [| signedStartInt |]
    let mutable i = 0
    while i < numberOfSlices do
      deltaEncodedIntList <- Array.append deltaEncodedIntList <| FrameOfReferenceIntListEncoder.read br
      i <- i + 1

    BigInteger.deltaDecodeInts deltaEncodedIntList

let decode (encodedBigInts: array<byte>): array<BigInteger> =
  let stream = new MemoryStream(encodedBigInts)
  let br = new BitReader(stream)
  let encoder = new BinaryPackingIntListEncoder()
  let bigInts = encoder.read br
  stream.Close()
  bigInts |> Seq.toArray

let encode (bigInts: array<BigInteger>): array<byte> =
  let bw = new BitWriter()
  let encoder = new BinaryPackingIntListEncoder()
  encoder.write bw bigInts
  bw.close()
  bw.toByteArray()