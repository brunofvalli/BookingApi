﻿namespace Ploeh.Samples.Booking.HttpApi.UnitTests

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
        let actual : DateTime seq = Dates.In(Year(year))

        let daysInYear = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInYear year
        Assert.Equal(DateTime(year, 1, 1), actual |> Seq.head)
        Assert.Equal(daysInYear, actual |> Seq.length)
        Assert.Equal(
            DateTime(year, 1, 1).AddDays (float daysInYear - 1.0),
            actual |> Seq.toList |> List.rev |> List.head)

    [<Theory; TestConventions>]
    let DatesInMonthReturnsCorrectResult (year : int) =
        let month = [1 .. 12] |> PickRandom

        let actual : DateTime seq = Dates.In(Month(year, month))

        let daysInMonth = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
        Assert.Equal(DateTime(year, month, 1), actual |> Seq.head)
        Assert.Equal(daysInMonth, actual |> Seq.length)
        Assert.Equal(
            DateTime(year, month, 1).AddDays (float daysInMonth - 1.0),
            actual |> Seq.toList |> List.rev |> List.head)

    [<Theory; TestConventions>]
    let DatesInDayReturnsCorrectResult (year : int) =
        let month = [1 .. 12] |> PickRandom
        let daysInMonth = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
        let day = [1 .. daysInMonth] |> PickRandom

        let actual = Dates.In(Day(year, month, day))

        let expected = DateTime(year, month, day)
        Assert.Equal(1, actual |> Seq.length)
        Assert.Equal(expected, actual |> Seq.head)

    [<Theory; TestConventions>]
    let BoundariesOfYearReturnsCorrectResult (year : int) =
        let actual : DateTime * DateTime = Dates.BoundariesIn(Year(year))

        let expectedMin = DateTime(year, 1, 1)
        let expectedMax = DateTime(year + 1, 1, 1).AddTicks -1L
        Assert.Equal((expectedMin, expectedMax), actual)

    [<Theory; TestConventions>]
    let BoundariesOfMonthReturnsCorrectResult (year : int) =
        let month = [1 .. 12] |> PickRandom

        let actual = Dates.BoundariesIn(Month(year, month))

        let expectedMin = DateTime(year, month, 1)
        let expectedMax = (expectedMin.AddMonths 1).AddTicks -1L
        Assert.Equal((expectedMin, expectedMax), actual)

    [<Theory; TestConventions>]
    let BoundariesOfDayReturnsCorrectResult (year : int) =
        let month = [1 .. 12] |> PickRandom
        let daysInMonth = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
        let day = [1 .. daysInMonth] |> PickRandom

        let actual = Dates.BoundariesIn(Day(year, month, day))

        let expectedMin = DateTime(year, month, day)
        let expectedMax = (expectedMin.AddDays 1.0).AddTicks -1L
        Assert.Equal((expectedMin, expectedMax), actual)

module ReserverationsTests =
    open Reservations

    [<Theory; TestConventions>]
    let ReservationsInMemoryAreReservations (sut : ReservationsInMemory) =
        Assert.IsAssignableFrom<IReservations>(sut)
    
    [<Theory; TestConventions>]
    let ToReservationsReturnsCorrectResult (expected : Envelope<Reservation> seq) =
        let actual : ReservationsInMemory = expected |> ToReservations
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

        let actual : Envelope<Reservation> seq = sut |> On date.Item.Date

        let expected =
            sut
            |> Between date.Item.Date.Date ((date.Item.Date.Date.AddDays 1.0) - TimeSpan.FromTicks 1L)
        Assert.Equal<Envelope<Reservation>>(expected, actual)

    [<Theory; TestConventions>]
    let HandleOnNoExistingReservationsReturnsCorrectResult(request : Envelope<MakeReservation>) =
        let capacity = request.Item.Quantity + 10
        let reservations = [] |> ToReservations
        let before = DateTimeOffset.Now

        let actual : Envelope<Reservation> option = Handle capacity reservations request
        
        Assert.True actual.IsSome
        Assert.Equal(request.Item.Date, actual.Value.Item.Date)
        Assert.Equal<string>(request.Item.Name, actual.Value.Item.Name)
        Assert.Equal<string>(request.Item.Email, actual.Value.Item.Email)
        Assert.Equal(request.Item.Quantity, actual.Value.Item.Quantity)
        Assert.NotEqual(Guid.Empty, actual.Value.Id)
        Assert.NotEqual(request.Id, actual.Value.Id)
        Assert.NotEqual(request.Created, actual.Value.Created)
        Assert.True(before <= actual.Value.Created)
        Assert.True(actual.Value.Created <= DateTimeOffset.Now)

    [<Theory; TestConventions>]
    let HandleOnSoldOutReturnsCorrectResult(request : Envelope<MakeReservation>,
                                            name : string,
                                            email : string,
                                            capacity : int) =
        let reservations =
            {
                Reservation.Date = request.Item.Date
                Name = name
                Email = email
                Quantity = capacity }
            |> EnvelopWithDefaults
            |> Seq.singleton
            |> ToReservations
        let actual = Handle capacity reservations request
        Assert.True actual.IsNone

    [<Theory; TestConventions>]
    let HandleOnAlmostSoldOutReturnsCorrectResult(requestSeed : Envelope<MakeReservation>,
                                                  name : string,
                                                  email : string) =
        let request = { requestSeed
                        with Item = { requestSeed.Item
                                      with Quantity =
                                            requestSeed.Item.Quantity + 1 } }
        let capacity = request.Item.Quantity + 10
        let reservations =
            {
                Reservation.Date = request.Item.Date
                Name = name
                Email = email
                Quantity = capacity - 1 }
            |> EnvelopWithDefaults
            |> Seq.singleton
            |> ToReservations

        let actual = Handle capacity reservations request
        
        Assert.True actual.IsNone

module NotificationsTest =
    open Notifications

    [<Theory; TestConventions>]
    let NotificationsInMemoryAreNotifications (sut : NotificationsInMemory) =
        Assert.IsAssignableFrom<INotifications> sut

    [<Theory; TestConventions>]
    let ToNotificationsReturnsCorrectResult (expected : Envelope<Notification> seq) =
        let actual : NotificationsInMemory = expected |> ToNotifications
        Assert.Equal<Envelope<Notification>>(expected, actual)

    [<Theory; TestConventions>]
    let InMemoryAboutWhenRequestedNotificationIsPresentReturnsCorrectResult
        (generator : Generator<Envelope<Notification>>) =

        let notifications = generator |> Seq.take 10 |> Seq.toList
        let expected = notifications |> PickRandom
        let sut = notifications |> ToNotifications

        let actual = (sut :> INotifications).About expected.Item.About

        Assert.Equal(1, actual |> Seq.length)
        Assert.Equal(expected, actual |> Seq.head)

    [<Theory; TestConventions>]
    let InMemoryAboutWhenRequestedNotificationIsNotPresentReturnsCorrectResult
        (generator : Generator<Envelope<Notification>>)
        (about : Guid) =

        let sut = generator |> Seq.take 10 |> Seq.toList |> ToNotifications
        Assert.False(sut |> Seq.exists (fun n -> n.Item.About = about))

        let actual = (sut :> INotifications).About about

        Assert.True(actual |> Seq.isEmpty)

    [<Theory; TestConventions>]
    let AboutReturnsCorrectResult(notifications : Envelope<Notification> seq) =
        let sut = notifications |> ToNotifications
        let about = (sut |> Seq.toList |> PickRandom).Item.About

        let actual : Envelope<Notification> seq = sut |> About about

        let expected = (sut :> INotifications).About about
        Assert.Equal<Envelope<Notification>>(expected, actual)
