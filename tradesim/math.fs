module dke.tradesim.Math

let ceil (d: decimal): decimal = System.Math.Ceiling(d)

let floor (d: decimal): decimal = System.Math.Floor(d)

let round (d: decimal): decimal = System.Math.Round(d)

let integralQuotient (dividend: decimal) (divisor: decimal): decimal = floor (dividend / divisor)

module Decimal = 

  let billionth = 0.000000001M

  let sum (xs: seq<decimal>): decimal = Seq.reduce (+) xs

  let isSignSame a b = (a < 0M) = (b < 0M)

  let isSignDiff a b = not (isSignSame a b)

  let rec pow (x: decimal) (exp: int): decimal =
    if exp > 0 then
      let mutable acc = 1M
      for i = 1 to exp do
        acc <- acc * x
      acc
    elif exp = 0 then
      1M
    else
      1M / pow x -exp

  // This is a recursive implementation of the Bisection Method as defined by pseudocode in
  // Bisection3 on page 95 of Numerical Mathematics and Computing (5th ed.) by Ward/Kincaid.
  //
  // Arguments:
  // a, b define the interval within which the root is guaranteed to exist (i.e. a < root < b)
  // fa, fb are f(a) and f(b) respectively
  // n-max is the maximum iterations to perform
  // epsilon is an error threshold. The algorithm continues iterating until the error is less than epsilon.
  // n is the current iteration
  //
  // Returns:
  // [root-approximation error number-of-iterations]
  let rec findRootWithBisectionMethodR (f: decimal -> decimal) (a: decimal) (b: decimal) (fa: decimal) (fb: decimal) (maxN: int) (epsilon: decimal) (n: int): decimal =
    let error = (b - a) / 2M     // error <- (b - a) / 2
    let c = a + error            // c <- a + error  (c is the midpoint between a and b ; this is our best root approximation)
    let fc = (f c)               // fc <- f(c)      (fc is f evaluated at the midpoint between a and b)
    let n' = n + 1
    if ((abs error) < epsilon) || (n' > maxN) then  // our error is less than the error threshold epsilon OR we've executed the maximum number of iterations - in either case we have converged enough, so return c
      c
    else
      if isSignDiff fa fc then
        findRootWithBisectionMethodR f a c fa fc maxN epsilon n'
      else
        findRootWithBisectionMethodR f c b fc fb maxN epsilon n'

  let findRootWithBisectionMethod5 f a b maxN epsilon: decimal = findRootWithBisectionMethodR f a b (f a) (f b) maxN epsilon 1
  let findRootWithBisectionMethod f a b: decimal = findRootWithBisectionMethod5 f a b 300 billionth

  // Returns the nth root of A
  // -> A^(1/n)
  // It works by finding the positive root (i.e. positive zero) of the function f(x) = x^n - A
  // It returns the positive x at which f(x) = 0.
  //
  // Arguments:
  // n is the root we want to find (i.e. the "n" in "nth root")
  // A is the positive real number that we want to find the nth root of.
  //
  // Usage:
  //   (nth-root 45.13579 3)
  //   -> 3.5604674194663124
  //   Check the result:
  //   (expt 3.5604674194663124 3)
  //   45.13578999552352        (that's pretty close!)
  //
  //   (nth-root 0.456 4)
  //   -> 882350387/1073741824       ; (float (nth-root 0.456 4)) = 0.82175285
  //   Check the result:
  //   (float (expt 882350387/1073741824 4))
  //   -> 0.456
  let nthRoot (a: decimal) (n: int): decimal =
    let f = (fun x -> (pow x n) - a)
    if a < 1M then
      findRootWithBisectionMethod f 0M 1M
    else
      findRootWithBisectionMethod f 0M a


  // todo: finish implementing this based on https://github.com/davidkellis/dke-contrib/blob/master/dke/contrib/math.clj
  // Approximate the definite integral of f from a to b, using Composite Simpson's rule
  // This is a Clojure port of the Python implementation at http://en.wikipedia.org/wiki/Simpson's_rule
  let integrateWithSimpsonsRuleR f a b n: decimal =
    let h = ((b - a) / n)       // factor this out of the loop for efficiency
    let sum = seq { 1 .. n } |> Seq.map (fun k -> ((k % 2) + 1) * (f ( a + (h * k)))) |> sum
    (h / 3) * ((f a) + (2 * sum) + (f b))

  let integrateWithSimpsonsRule f a b = integrateWithSimpsonsRuleR f a b 100