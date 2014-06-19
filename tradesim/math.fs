module dke.tradesim.Math

let ceil (d: decimal): decimal = System.Math.Ceiling(d)

let floor (d: decimal): decimal = System.Math.Floor(d)

let round (d: decimal): decimal = System.Math.Round(d)

let integralQuotient (dividend: decimal) (divisor: decimal): decimal = floor (dividend / divisor)