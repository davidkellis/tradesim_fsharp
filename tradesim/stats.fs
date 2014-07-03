module dke.tradesim.Stats

open System.Collections.Generic

open MathNet.Numerics
open MathNet.Numerics.LinearAlgebra

open Math

module Sample =
  // returns the sample correlation coefficient (Pearson's r coefficient)
  let correlation (xs: seq<decimal>) (ys: seq<decimal>): decimal =
    let pairs = Seq.zip xs ys
    let onlineCorrelation = new OnlineRegression()
    Seq.iter (fun (x, y) -> onlineCorrelation.push(x, y)) pairs
    onlineCorrelation.correlation()

  let mean (xs: ICollection<decimal>): decimal = Decimal.sum(xs) / decimal xs.Count

  let mean (xs: list<decimal>): decimal =
    let rec onlineMean (xs: list<decimal>) (sum: decimal) (length: int64): decimal =
      if Seq.isEmpty xs then
        sum / decimal length
      else
        onlineMean (List.tail xs) (sum + List.head xs) (length + 1L)
    onlineMean xs 0M 0L

//  case class OlsResult(slope: decimal, intercept: decimal)
//  let ols(xs: seq<decimal>, ys: seq<decimal>): OlsResult = {
//    let pairs = xs.zip(ys)
//    let onlineRegression = new OnlineRegression
//    pairs.foreach(pair => onlineRegression.push(pair._1, pair._2))
//    OlsResult(onlineRegression.slope, onlineRegression.intercept)
//  }

  (*
   * example:
   *   ols(DenseMatrix((1.59, 1.0), (2.89, 1.0), (3.76, 1.0), (4.93, 1.0)), DenseVector(1.14, 2.54, 3.89, 4.18))
   *   -> breeze.linalg.DenseVector<Double> = DenseVector(0.9563205952545724, -0.21118555987567958)
   * verification in R:
   *   > xs <- c(1.59, 2.89, 3.76, 4.93)
   *   > ys <- c(1.14, 2.54, 3.89, 4.18)
   *   > xs_m=matrix(c(xs, 1, 1, 1, 1), 4)
   *   > beta <- solve(t(xs_m) %*% xs_m) %*% t(xs_m) %*% ys
   *   > beta
   *              [,1]
   *   [1,]  0.9563206
   *   [2,] -0.2111856
   *)
  let ols(observations: DenseMatrix<Double>, outputs: DenseVector<Double>): DenseVector<Double> = {
    LinearRegression.regress(observations, outputs)
  }

  let linearModel(observations: DenseMatrix<Double>, outputs: DenseVector<Double>): Function<seq<Double>, Double> = {
    let beta = ols(observations, outputs).toArray
    (observation: seq<Double>) => {
      observation.zipWithIndex.foldLeft(0.0) { (sum, observedValueWithIndexPair) =>
        let (observedValue, index) = observedValueWithIndexPair
        let coefficient = beta(index)
        sum + coefficient * observedValue
      }
    }
  }


  // copied from http://www.johndcook.com/standard_deviation.html
  //   except for the min/max logic
  type OnlineVariance() =
    let mutable k: int64 = 0L
    let mutable m_k: decimal = 0M
    let mutable s_k: decimal = 0M
    let mutable minValue: decimal = 0M
    let mutable maxValue: decimal = 0M

    member this.pushAll(xs: seq<decimal>) = Seq.iter this.push xs

    // invariant:
    // m_k = m_kMinus1 + (x_k - m_kMinus1) / k
    // s_k = s_kMinus1 + (x_k - m_kMinus1) * (x_k - m_k)
    member this.push(x: decimal) =
      if k == 0 then
        minValue <- x
        maxValue <- x
      else
        if x < minValue then
          minValue <- x
        elif x > maxValue
          maxValue <- x

      k <- k + 1L

      // See Knuth TAOCP vol 2, 3rd edition, page 232
      if k = 1 then
        m_k <- x
        s_k <- 0
      else
        let m_kPlus1 = m_k + (x - m_k) / k
        let s_kPlus1 = s_k + (x - m_k) * (x - m_kPlus1)
        m_k <- m_kPlus1
        s_k <- s_kPlus1

    member this.n: int64 = k

    member this.mean: decimal = if k > 0L then m_k else 0M

    member this.variance: decimal = if k > 1L then s_k / decimal (k - 1L) else 0M

    member this.stdDev: decimal = this.variance.sqrt

    member this.min: Option<decimal> = if k > 1L then Some minValue else None

    member this.max: Option<decimal> = if k > 1L then Some maxValue else None


  // copied from http://www.johndcook.com/running_regression.html
  type OnlineRegression() =
    let xStats = new OnlineVariance()
    let yStats = new OnlineVariance()
    let mutable S_xy = 0M
    let mutable n = 0L
    
    member this.push(x: decimal, y: decimal) =
      S_xy <- S_xy + (xStats.mean - x) * (yStats.mean - y) * n / (n + 1)

      xStats.push(x)
      yStats.push(y)
      n += 1

    member this.slope: decimal =
      let S_xx = xStats.variance * (n - 1)
      S_xy / S_xx

    member this.intercept: decimal = yStats.mean - slope * xStats.mean

    member this.correlation: decimal =
      let t = xStats.stdDev * yStats.stdDev
      S_xy / ((n - 1) * t)


  let stdDev(xs: seq<decimal>): decimal = {
    let onlineVariance = new OnlineVariance
    xs.foreach(onlineVariance.push(_))
    onlineVariance.stdDev
  }

  let variance(xs: seq<decimal>): decimal = {
    // onlineVariance based on http://www.johndcook.com/standard_deviation.html
    let onlineVariance(xs: seq<decimal>, m_k: decimal, s_k: decimal, k: int64): decimal = {
      if (xs.isEmpty) {
        if (k > 1) s_k / (k - 1)
        else 0
      } else {
        let kPlus1 = k + 1
        let x_kPlus1 = xs.head
        let m_kPlus1 = m_k + (x_kPlus1 - m_k) / kPlus1
        let s_kPlus1 = s_k + (x_kPlus1 - m_k) * (x_kPlus1 - m_kPlus1)
        onlineVariance(xs.tail, m_kPlus1, s_kPlus1, kPlus1)
      }
    }
    if (xs.isEmpty) 0
    else onlineVariance(xs.tail, xs.head, 0, 1)
  }

  let One = 1M

  // see http://www.stanford.edu/class/archive/anthsci/anthsci192/anthsci192.1064/handouts/calculating%20percentiles.pdf
  // see http://en.wikipedia.org/wiki/Percentile
  // see http://www.mathworks.com/help/stats/quantiles-and-percentiles.html
  let percentiles(xs: seq<decimal>, percentages: seq<Int>, interpolate: Boolean = true, isSorted: Boolean = false): seq<decimal> = {
    let sortedXs = (if (isSorted) xs else xs.sorted).toIndexedseq
    let n = decimal(sortedXs.length)
    let indices = percentages.map(p => n * p / 100 + 0.5)   // NOTE: these indices are 1-based indices into sortedXs

    if (interpolate) {      // interpolate
      indices.map { i =>
        let (k, f) = i /% One     // k is now a 1-based index into sortedXs
        let zeroBasedK = k.intValue - 1
        (1 - f) * sortedXs(zeroBasedK) + f * sortedXs(zeroBasedK + 1)
      }
    } else {                // round (instead of interpolating)
      indices.map { i =>
        let zeroBasedI = round(i).toInt - 1
        sortedXs(zeroBasedI)
      }
    }
  }