module dke.tradesim.Math

let ceil (d: decimal): decimal = System.Math.Ceiling(d)

let floor (d: decimal): decimal = System.Math.Floor(d)

let round (d: decimal): decimal = System.Math.Round(d)

let integralQuotient (dividend: decimal) (divisor: decimal): decimal = floor (dividend / divisor)

module Decimal = 

  let billionth = 0.000000001m

  let sum (xs: seq<decimal>): decimal = Seq.reduce (+) xs

  let isSignSame a b = (a < 0m) = (b < 0m)

  let isSignDiff a b = not (isSignSame a b)

  let rec pow (x: decimal) (exp: int): decimal =
    if exp > 0 then
      let mutable acc = 1m
      for i = 1 to exp do
        acc <- acc * x
      acc
    elif exp = 0 then
      1m
    else
      1m / pow x -exp

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
    let error = (b - a) / 2m     // error <- (b - a) / 2
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
    if a < 1m then
      findRootWithBisectionMethod f 0m 1m
    else
      findRootWithBisectionMethod f 0m a
  
  let sqrt n = nthRoot n 2

  // Approximate the definite integral of f from a to b, using Composite Simpson's rule: http://en.wikipedia.org/wiki/Simpson's_rule#Composite_Simpson.27s_rule
  // This implementation implements:
  //   integral from a to b of f(x) dx is approximated by
  //   h/3 * [ f(x_0) + 4f(x_1) + 2f(x_2) + 4f(x_3) + 2f(x_4) + ... + 4f(x_n-1) + f(x_n) ]
  //   where h = (b-a)/n and x_i = a + i*h for i = 0, 1, ..., n-1, n; x_0 = a + 0*h = a; x_n = a + n*h = a + n((b-a)/n) = b
  // NOTE: numberOfSubintervalsN must be even!!!
  // Example:
  //   On Wolfram Alpha, evaluate: Simpson's rule 2+1/(sqrt(x))+1/(4*x) on [2,5] with interval size 0.5:
  //
  //   let f (x: decimal) = 2m + (1m / sqrt x) + (1m / (4m * x))
  //   integrateWithCompositeSimpsonsRule f 2m 5m 6
  //   val it : decimal = 7.8728590394739751961083936282M
  //
  //   On Wolfram Alpha, evaluate: integral (x^2-2)/x dx from 1 to 2 using Boole's rule:
  //
  //   let f (x: decimal) = (pow x 2 - 2m) / x
  //   integrateWithCompositeSimpsonsRule f 1m 2m 6
  //   val it : decimal = 0.1136604136604136604136604139M
  let integrateWithCompositeSimpsonsRule (f: decimal -> decimal) (a: decimal) (b: decimal) (numberOfSubintervalsN: int): decimal =
    let h = ((b - a) / decimal numberOfSubintervalsN)       // factor this out of the loop for efficiency
    let summation = seq { 1 .. (numberOfSubintervalsN - 1) } |> Seq.map (fun k -> (decimal ((k % 2) + 1)) * f (a + (h * decimal k))) |> sum
    (h / 3m) * ((f a) + (2m * summation) + (f b))

  let integrateWithSimpsonsRule f a b = integrateWithCompositeSimpsonsRule f a b 100