namespace Ploeh.Samples.Booking.HttpApi.UnitTests

open System
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.UnitTests.TestDsl
open Xunit
open Xunit.Extensions

module DatesTests =
    [<Theory; TestConventions>]
    let DatesFromReturnsCorrectSequence (dates : DateTime seq) =
        let sortedDates =
            dates |> Seq.map (fun d -> d.Date) |> Seq.sort |> Seq.toList
        let head = sortedDates |> List.head
        let last = sortedDates |> Seq.nth 1

        let actual : DateTime seq = Dates.InitInfinite head

        Assert.Equal(head, actual |> Seq.head)
        Assert.Equal(last.AddDays -1.0, actual
                                        |> Seq.takeWhile (fun d -> d <> last)
                                        |> Seq.toList
                                        |> List.rev
                                        |> List.head)

    [<Theory; TestConventions>]
    let DatesInYearReturnsCorrectResult (year : int) =
        let actual : DateTime seq = Dates.InYear year

        let daysInYear = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInYear year
        Assert.Equal(DateTime(year, 1, 1), actual |> Seq.head)
        Assert.Equal(daysInYear, actual |> Seq.length)
        Assert.Equal(
            DateTime(year, 1, 1).AddDays (float daysInYear - 1.0),
            actual |> Seq.toList |> List.rev |> List.head)

    [<Theory; TestConventions>]
    let DatesInMonthReturnsCorrectResult (year : int) =
        let month = [1 .. 12] |> PickRandom

        let actual : DateTime seq = Dates.InMonth year month

        let daysInMonth = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
        Assert.Equal(DateTime(year, month, 1), actual |> Seq.head)
        Assert.Equal(daysInMonth, actual |> Seq.length)
        Assert.Equal(
            DateTime(year, month, 1).AddDays (float daysInMonth - 1.0),
            actual |> Seq.toList |> List.rev |> List.head)