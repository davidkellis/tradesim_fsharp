module dke.tradesim.SpecialDates

open Time
open NodaTime

let newYears year: LocalDate = date year 1 1

// third monday in January in the given year
let martinLutherKingJrDay = nthWeekdayOfMonth 3 DayOfWeek.Monday Month.January

(*
 * president's day - third monday in February in the given year
 *
 * NOTE: Washington's Birthday was first declared a federal holiday by an 1879 act of Congress. 
 * The Monday Holiday Law, enacted in 1968, shifted the date of the commemoration of Washington's Birthday 
 * from February 22 to the third Monday in February, but neither that law nor any subsequent law changed 
 * the name of the holiday from Washington's Birthday to President's Day. Although the third Monday in 
 * February has become popularly known as President's Day, the NYSE's designation of Washington's Birthday 
 * as an Exchange holiday (Rule 51) follows the form of the federal holiday outlined above (section 6103(a) 
 * of title 5 of the United States Code).
 *)
let presidentsDay = nthWeekdayOfMonth 3 DayOfWeek.Monday Month.February

// last Monday in May
let memorialDay = lastWeekdayOfMonth DayOfWeek.Monday Month.May

// July 4th
let independenceDay year = date year (int Month.July) 4

// first Monday in September
let laborDay = nthWeekdayOfMonth 1 DayOfWeek.Monday Month.September

// second Monday in October
// NOTE: CBOE still observes Columbus day, but NYSE does not (NYSE observed it between 1909-1953).
let columbusDay = nthWeekdayOfMonth 2 DayOfWeek.Monday Month.October

// fourth Thursday in November
let thanksgiving = nthWeekdayOfMonth 4 DayOfWeek.Thursday Month.November

let christmas year = date year (int Month.December) 25

(*
 * This is a non-trivial calculation. See http://en.wikipedia.org/wiki/Computus
 *   "Computus (Latin for "computation") is the calculation of the date of Easter in the Christian calendar."
 *   Evidently the scientific study of computation (or Computer Science, as we like to call it) was born out
 *   of a need to calculate when easter was going to be.
 * See http://www.linuxtopia.org/online_books/programming_books/python_programming/python_ch38.html
 * There is also a clojure version (that doesn't work properly) at: http://www.bitshift.me/calculate-easter-in-clojure/
 * The following code was taken from: http://www.merlyn.demon.co.uk/estralgs.txt
 * function McClendon(YR) {
 *   var g, c, x, z, d, e, n
 *   g = YR % 19 + 1   // Golden
 *   c = ((YR/100)|0) + 1    // Century
 *   x = ((3*c/4)|0) - 12    // Solar
 *   z = (((8*c+5)/25)|0) - 5  // Lunar
 *   d = ((5*YR/4)|0) - x - 10 // Letter ?
 *   e = (11*g + 20 + z - x) % 30  // Epact
 *   if (e<0) e += 30    // Fix 9006 problem
 *   if ( ( (e==25) && (g>11) ) || (e==24) ) e++
 *   n = 44 - e
 *   if (n<21) n += 30   // PFM
 *   return n + 7 - ((d+n)%7)  // Following Sunday
 *   }
 *)
let easter year: LocalDate =
  let g = year % 19 + 1
  let c = year / 100 + 1
  let x = (3 * c / 4) - 12
  let z = (8 * c + 5) / 25 - 5
  let d = 5 * year / 4 - x - 10
  let e = (11 * g + 20 + z - x) % 30
  let e1 = if e < 0 then e + 30 else e
  let e2 = if (e1 = 25 && g > 11) || e1 = 24 then e1 + 1 else e1
  let n = 44 - e2
  let n1 = if n < 21 then n + 30 else n
  let n2 = (n1 + 7) - ((d + n1) % 7)
  let day = if n2 > 31 then n2 - 31 else n2
  let month = if n2 > 31 then 4 else 3
  date year month day

// the Friday before Easter Sunday
let goodFriday year: LocalDate = easter year - Period.FromDays(2L)

(*
 * holidayFn is a function of an integer year that returns a LocalDate representing the date
 * that the holiday falls on in that year
 * Example: isHoliday(datetimeUtils(2012, 1, 16), martinLutherKingJrDay) => true
 *)
let isHoliday (date: LocalDate) (holidayFn: int -> LocalDate): bool = holidayFn date.Year = date

let HolidayLookupFunctions = [
    newYears;
    martinLutherKingJrDay;
    presidentsDay;
    goodFriday;
    memorialDay;
    independenceDay;
    laborDay;
//    columbusDay;    // CBOE still observes Columbus day, but NYSE does not (NYSE observed it between 1909-1953).
    thanksgiving;
    christmas
  ]

let isAnyHoliday (date: LocalDate): bool = List.exists (isHoliday date) HolidayLookupFunctions

(*
 * sources:
 * http://www1.nyse.com/pdfs/closings.pdf
 * http://www.cboe.com/publish/RegCir/RG12-150.pdf
 *)
let UnscheduledMarketClosures =
  [
    date 1972 12 28;    // Closed. Funeral of former President Harry S. Truman.
    date 1973 1 25;     // Closed for funeral of former President Lyndon B. Johnson.
    date 1977 7 14;     // Closed due to blackout in New York City.
    date 1985 9 27;     // Market closed due to Hurricane Gloria.
    date 1994 4 27;     // Closed for funeral of former President Richard M. Nixon.
    date 2001 9 11;     // Closed following the terrorist attack on the World Trade Center. 
    date 2001 9 12;     // Closed following the terrorist attack on the World Trade Center. 
    date 2001 9 13;     // Closed following the terrorist attack on the World Trade Center. 
    date 2001 9 14;     // Closed following the terrorist attack on the World Trade Center. 
    date 2004 6 11;     // Closed in observance of the National Day of Mourning for former President Ronald W. Reagan (died June 5, 2004)
    date 2007 1 2;      // Closed in observance of the National Day of Mourning for former President Gerald R. Ford (died December 26, 2006).
    date 2012 10 29;    // Closed Monday for Hurricane Sandy
    date 2012 10 30     // Closed Tuesday for Hurricane Sandy
  ] |> Set.ofList

let isDateUnscheduledMarketClosure date = Set.contains date UnscheduledMarketClosures 

// see the holiday rules at: http://cfe.cboe.com/aboutcfe/ExpirationCalendar.aspx
let isMarketHoliday includeUnscheduledMarketClosures date =
  let todayIsUnscheduledClosure = if includeUnscheduledMarketClosures then isDateUnscheduledMarketClosure date else false
  let todayIsFridayAndSaturdayIsHoliday = isDateFriday date && (nextDay date |> isAnyHoliday)
  let todayIsMondayAndSundayIsHoliday = isDateMonday date && (previousDay date |> isAnyHoliday)
  isAnyHoliday date || todayIsFridayAndSaturdayIsHoliday || todayIsMondayAndSundayIsHoliday || todayIsUnscheduledClosure

let isMarketBusinessDay includeUnscheduledMarketClosures date = 
  isBusinessDay date && (not <| isMarketHoliday includeUnscheduledMarketClosures date)