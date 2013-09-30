namespace Ploeh.Samples.Booking.HttpApi.UnitTests

open System
open Xunit
open Xunit.Extensions
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.UnitTests.TestDsl
open Ploeh.AutoFixture

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

module ReserverationsTests =
    open Reserverations

    [<Theory; TestConventions>]
    let ReservationsInMemoryAreReservations (sut : InMemory) =
        Assert.IsAssignableFrom<IReservations>(sut)
    
    [<Theory; TestConventions>]
    let ToReservationsReturnsCorrectResult (expected : Envelope<Reservation> seq) =
        let actual : InMemory = expected |> ToReservations
        Assert.Equal<Envelope<Reservation>>(expected, actual)

    [<Theory; TestConventions>]
    let InMemoryBetweenReturnsCorrectResult (generator : Generator<Envelope<Reservation>>) =
        let reservations =
            generator |> Seq.take 10 |> Seq.sortBy (fun r -> r.Item.Date) |> Seq.toArray
        let min = reservations.[2]
        let max = reservations.[7]
        let expected = reservations |> Seq.skip 2 |> Seq.take 6
        let sut = reservations |> ToReservations

        let actual = (sut :> IReservations).Between min.Item.Date max.Item.Date

        Assert.Equal<Envelope<Reservation>>(expected, actual)

    [<Theory; TestConventions>]
    let BetweenReturnsCorrectResult (generator : Generator<Envelope<Reservation>>) =
        let reservations =
            generator |> Seq.take 10 |> Seq.sortBy (fun r -> r.Item.Date) |> Seq.toArray
        let min = reservations.[2]
        let max = reservations.[7]
        let sut = reservations |> ToReservations

        let actual = sut |> Between min.Item.Date max.Item.Date

        let expected = (sut :> IReservations).Between min.Item.Date max.Item.Date
        Assert.Equal<Envelope<Reservation>>(expected, actual)

    [<Theory; TestConventions>]
    let OnReturnsCorrectResult (reservations : Envelope<Reservation> array) =
        let date = reservations |> Array.toList |> PickRandom
        let sut = reservations |> ToReservations

        let actual : Envelope<Reservation> seq = sut |> On date.Item

        let expected =
            sut
            |> Between date.Item.Date.Date ((date.Item.Date.Date.AddDays 1.0) - TimeSpan.FromTicks 1L)
        Assert.Equal<Envelope<Reservation>>(expected, actual)