module dke.tradesim.MInterval

open NodaTime

open Time

type MInterval = array<Interval>

let emptyMInterval: MInterval = [| |]

let createMInterval (intervals: seq<Interval>): MInterval = Seq.sortBy (fun (i: Interval) -> i.Start) intervals |> Seq.toArray

// Returns a vector of intervals (a.k.a. a vinterval), each representing a portion of the remaining interval after
//   the subtrahend interval has been subtracted from the minuend interval.
//   In other words, (subtract-interval b c) === c - b.
// The returned vector will consist of 0, 1, or 2 interval objects.
//   An empty return vector represents an empty interval.
// Note (source: http://en.wikipedia.org/wiki/Subtraction):
//   Since subtraction is not a commutative operator, the two operands are named.
//   The traditional names for the parts of the formula
//   c − b = a
//   are minuend (c) − subtrahend (b) = difference (a).
let subtractInterval (minuend: Interval) (subtrahend: Interval): MInterval =
  if intervalsOverlap subtrahend minuend then
    let startMinuend = minuend.Start |> instantToEasternTime
    let endMinuend = minuend.End |> instantToEasternTime
    let startSubtrahend = subtrahend.Start |> instantToEasternTime
    let endSubtrahend = subtrahend.End |> instantToEasternTime

    match compareDateTimes startMinuend startSubtrahend with
    | i when i < 0 ->                                           // startMinuend < startSubtrahend
      match compareDateTimes endMinuend endSubtrahend with
      | i when i < 0 ->                                         //   endMinuend < endSubtrahend
        [| intervalBetween startMinuend startSubtrahend |]
      | i when i > 0 ->                                         //   endMinuend > endSubtrahend
        [| intervalBetween startMinuend startSubtrahend; intervalBetween endSubtrahend endMinuend |]
      | 0 ->                                                    //   endMinuend == endSubtrahend
        [| intervalBetween startMinuend startSubtrahend |]
      | _ -> failwith "subtractInterval failed: compareDateTimes returned an unexpected value."
    | i when i > 0 ->                                           // startMinuend > startSubtrahend
      match compareDateTimes endMinuend endSubtrahend with
      | i when i < 0 ->                                         //   endMinuend < endSubtrahend
        emptyMInterval
      | i when i > 0 ->                                         //   endMinuend > endSubtrahend
        [| intervalBetween endSubtrahend endMinuend |]
      | 0 ->                                                    //   endMinuend == endSubtrahend
        emptyMInterval
      | _ -> failwith "subtractInterval failed: compareDateTimes returned an unexpected value."
    | 0 ->                                                      // startMinuend == startSubtrahend
      match compareDateTimes endMinuend endSubtrahend with
      | i when i < 0 ->                                         //   endMinuend < endSubtrahend
        emptyMInterval
      | i when i > 0 ->                                         //   endMinuend > endSubtrahend
        [| intervalBetween endSubtrahend endMinuend |]
      | 0 ->                                                    //   endMinuend == endSubtrahend
        emptyMInterval
      | _ -> failwith "subtractInterval failed: compareDateTimes returned an unexpected value."
    | _ -> failwith "subtractInterval failed: compareDateTimes returned an unexpected value."
  else
    [| minuend |]

// represents the computation: minuend - subtrahend = difference
let subtractMInterval (minuend: MInterval) (subtrahend: MInterval): MInterval = 
  let mintervals = seq {
    for m in minuend do
    for s in subtrahend do
    yield! subtractInterval m s
  }
  createMInterval mintervals

let isMIntervalEmpty (mInterval: MInterval): bool = Array.isEmpty mInterval