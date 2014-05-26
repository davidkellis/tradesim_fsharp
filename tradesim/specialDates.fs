module dke.tradesim.SpecialDates

open Time
open NodaTime

let newYears year: LocalDate = date year 1 1

// third monday in January in the given year
let martinLutherKingJrDay = nthWeekday 3 DayOfWeek.Monday Month.January

// third monday in February in the given year
let presidentsDay = nthWeekday 3 DayOfWeek.Monday Month.February

// last Monday in May
let memorialDay = lastWeekday DayOfWeek.Monday Month.May

// July 4th
let independenceDay year = date year (int Month.July) 4

// first Monday in September
let laborDay = nthWeekday 1 DayOfWeek.Monday Month.September

// fourth Thursday in November
let thanksgiving = nthWeekday 4 DayOfWeek.Thursday Month.November

let christmas year = date year (int Month.December) 25
