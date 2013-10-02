namespace Ploeh.Samples.Booking.HttpApi.UnitTests

open System
open System.Net
open System.Net.Http
open System.Web.Http
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.UnitTests.TestDsl
open Xunit
open Xunit.Extensions
open Ploeh.AutoFixture
open Ploeh.AutoFixture.Xunit

module HomeControllerTests =
    [<Theory; TestConventions>]
    let SutIsController (sut : HomeController) =
        Assert.IsAssignableFrom<ApiController> sut

    [<Theory; TestConventions>]
    let GetReturnsCorrectResult (sut : HomeController) =    
        let actual : HttpResponseMessage = sut.Get()
    
        Assert.True(
            actual.IsSuccessStatusCode,
            "Actual status code: " + actual.StatusCode.ToString())

module ReservationRequestsControllerTests =
    [<Theory; TestConventions>]
    let SutIsController (sut : ReservationsController) =
        Assert.IsAssignableFrom<ApiController> sut

    [<Theory; TestConventions>]
    let PostReturnsCorrectResult(sut : ReservationsController,
                                 rendition : MakeReservationRendition) =
        let actual : HttpResponseMessage = sut.Post rendition

        Assert.Equal(HttpStatusCode.Accepted, actual.StatusCode)

    [<Theory; TestConventions>]
    let SutIsObservable (sut : ReservationsController) =
        Assert.IsAssignableFrom<IObservable<Envelope<MakeReservation>>> sut

    [<Theory; TestConventions>]
    let PostPublishesCorrectCommand(sut : ReservationsController,
                                    rendition : MakeReservationRendition) =        
        let verified = ref false
        let expected = {
            MakeReservation.Date = DateTime.Parse rendition.Date
            Name = rendition.Name
            Email = rendition.Email
            Quantity = rendition.Quantity }
        use sub = sut.Subscribe (fun cmd -> verified := expected = cmd.Item)

        sut.Post rendition |> ignore

        Assert.True(!verified, "Command should be published")

module AvailabilityControllerTests =
    [<Theory; TestConventions>]
    let SutIsController (sut : AvailabilityController) =
        Assert.IsAssignableFrom<ApiController> sut

    [<Theory; TestConventions>]
    let GetUnreservedFutureYearReturnsCorrectResult(sut : AvailabilityController,
                                                    yearsInFuture : int) =
        let year = DateTime.Now.Year + yearsInFuture

        let response : HttpResponseMessage = sut.Get year
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expectedRecords =
            Dates.In(Year(year))
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = sut.SeatingCapacity })
            |> Seq.toArray
        let expected = { Openings = expectedRecords }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetPastYearReturnsCorrectResult(sut : AvailabilityController,
                                          yearsInPast : int) =
        let year = DateTime.Now.Year - yearsInPast

        let response = sut.Get year
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expectedOpenings =
            Dates.In(Year(year))
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = 0 })
            |> Seq.toArray
        let expected = { Openings = expectedOpenings }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetCurrentUnreservedYearReturnsCorrectResult(sut : AvailabilityController) =
        let now = DateTimeOffset.Now
        let year = now.Year

        let response = sut.Get year
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expectedOpenings =
            Dates.In(Year(year))
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = if d < now.Date then 0 else sut.SeatingCapacity })
            |> Seq.toArray
        let expected = { Openings = expectedOpenings }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetYearWithReservationsReturnsCorrectResult(fixture : IFixture,
                                                    mutableReservations : System.Collections.Generic.List<Envelope<Reservation>>,
                                                    yearsInFuture : int) =
        // Fixture setup
        let reservations = mutableReservations |> Reservations.ToReservations
        fixture.Inject<Reservations.IReservations> reservations
        let sut =
            fixture.Generate<AvailabilityController>()
            |> Seq.filter (fun c -> c.SeatingCapacity > 1)
            |> Seq.head
        
        let year = DateTime.Now.Year + yearsInFuture
        let month = [1 .. 12] |> PickRandom
        let daysInMonth = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
        let day = [1 .. daysInMonth] |> PickRandom
        let reservationInYear =
            { fixture.Create<Reservation>() with
                Date = DateTime(year, month, day)
                Quantity = sut.SeatingCapacity - 1 }
            |> EnvelopWithDefaults
        mutableReservations.Add reservationInYear

        // Exercise SUT
        let response = sut.Get year
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result        
        
        // Verify outcome
        let expectedOpenings =
            Dates.In(Year(year))
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats =
                        if d = reservationInYear.Item.Date
                        then sut.SeatingCapacity - reservationInYear.Item.Quantity
                        else sut.SeatingCapacity })
            |> Seq.toArray
        let expected = { Openings = expectedOpenings }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetUnreservedFutureMonthReturnsCorrectResult(sut : AvailabilityController,
                                                     yearsInFuture : int) =
        let year = DateTime.Now.Year + yearsInFuture
        let month = [1 .. 12] |> PickRandom

        let response : HttpResponseMessage = sut.Get(year, month)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expectedOpenings =
            Dates.In(Month(year, month))
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = sut.SeatingCapacity })
            |> Seq.toArray
        let expected = { Openings = expectedOpenings }
        Assert.Equal(expected, actual)    
    
    [<Theory; TestConventions>]
    let GetPastMonthReturnsCorrectResult(sut : AvailabilityController,
                                         yearsInPast : int) =
        let year = DateTime.Now.Year - yearsInPast
        let month = [1 .. 12] |> PickRandom

        let response = sut.Get(year, month)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expectedOpenings =
            Dates.In(Month(year, month))
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = 0 })
            |> Seq.toArray
        let expected = { Openings = expectedOpenings }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetCurrentUnreservedMonthReturnsCorrectResult(sut : AvailabilityController) =
        let now = DateTimeOffset.Now
        let (year, month) = (now.Year, now.Month)

        let response = sut.Get(year, month)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expectedOpenings =
            Dates.In(Month(year, month))
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = if d < now.Date then 0 else sut.SeatingCapacity })
            |> Seq.toArray
        let expected = { Openings = expectedOpenings }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetMonthWithReservationsReturnsCorrectResult(fixture : IFixture,
                                                     mutableReservations : System.Collections.Generic.List<Envelope<Reservation>>,
                                                     yearsInFuture : int) =
        // Fixture setup
        let reservations = mutableReservations |> Reservations.ToReservations
        fixture.Inject<Reservations.IReservations> reservations
        let sut =
            fixture.Generate<AvailabilityController>()
            |> Seq.filter (fun c -> c.SeatingCapacity > 1)
            |> Seq.head
        
        let year = DateTime.Now.Year + yearsInFuture
        let month = [1 .. 12] |> PickRandom
        let daysInMonth = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
        let day = [1 .. daysInMonth] |> PickRandom
        let reservationInMonth =
            { fixture.Create<Reservation>() with
                Date = DateTime(year, month, day)
                Quantity = sut.SeatingCapacity - 1 }
            |> EnvelopWithDefaults
        mutableReservations.Add reservationInMonth

        // Exercise SUT
        let response = sut.Get(year, month)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result        
        
        // Verify outcome
        let expectedOpenings =
            Dates.In(Month(year, month))
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats =
                        if d = reservationInMonth.Item.Date
                        then sut.SeatingCapacity - reservationInMonth.Item.Quantity
                        else sut.SeatingCapacity })
            |> Seq.toArray
        let expected = { Openings = expectedOpenings }
        Assert.Equal(expected, actual)
    
    [<Theory; TestConventions>]
    let GetUnreservedFutureDayReturnsCorrectResult(sut : AvailabilityController,
                                                   yearsInFuture : int) =
        let year = DateTime.Now.Year + yearsInFuture
        let month = [1 .. 12] |> PickRandom
        let daysInMonth = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
        let day = [1 .. daysInMonth] |> PickRandom

        let response : HttpResponseMessage = sut.Get(year, month, day)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expected = {
            Openings =
                [| {
                    Date = DateTime(year, month, day).ToString "yyyy.MM.dd"
                    Seats = sut.SeatingCapacity }
                |] }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetPastDayReturnsCorrectResult(sut : AvailabilityController,
                                       yearsInPast : int) =
        let year = DateTime.Now.Year - yearsInPast
        let month = [1 .. 12] |> PickRandom
        let daysInMonth = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
        let day = [1 .. daysInMonth] |> PickRandom

        let response = sut.Get(year, month, day)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expected = {
            Openings =
                [| {
                    Date = DateTime(year, month, day).ToString "yyyy.MM.dd"
                    Seats = 0 }
                |] }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetCurrentUnreservedDayReturnsCorrectResult(sut : AvailabilityController) =
        let now = DateTimeOffset.Now
        let (year, month, day) = (now.Year, now.Month, now.Day)

        let response = sut.Get(year, month, day)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expected = {
            Openings =
                [| {
                    Date = DateTime(year, month, day).ToString "yyyy.MM.dd"
                    Seats = sut.SeatingCapacity }
                |] }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetYesterdayReturnsCorrectResult(sut : AvailabilityController) =
        let yesterday = DateTimeOffset.Now.Subtract(TimeSpan.FromDays 1.0)
        let (year, month, day) = (yesterday.Year, yesterday.Month, yesterday.Day)

        let response = sut.Get(year, month, day)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expected = {
            Openings =
                [| {
                    Date = DateTime(year, month, day).ToString "yyyy.MM.dd"
                    Seats = 0 }
                |] }
        Assert.Equal(expected, actual)    

    [<Theory; TestConventions>]
    let GetDayWithReservationsReturnsCorrectResult(fixture : IFixture,
                                                   mutableReservations : System.Collections.Generic.List<Envelope<Reservation>>,
                                                   yearsInFuture : int) =
        // Fixture setup
        let reservations = mutableReservations |> Reservations.ToReservations
        fixture.Inject<Reservations.IReservations> reservations
        let sut =
            fixture.Generate<AvailabilityController>()
            |> Seq.filter (fun c -> c.SeatingCapacity > 1)
            |> Seq.head
        
        let year = DateTime.Now.Year + yearsInFuture
        let month = [1 .. 12] |> PickRandom
        let daysInMonth = System.Globalization.CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
        let day = [1 .. daysInMonth] |> PickRandom
        let reservationOnDay =
            { fixture.Create<Reservation>() with
                Date = DateTime(year, month, day)
                Quantity = sut.SeatingCapacity - 1 }
            |> EnvelopWithDefaults
        mutableReservations.Add reservationOnDay

        // Exercise SUT
        let response = sut.Get(year, month, day)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result        
        
        // Verify outcome
        let expected = {
            Openings =
                [| {
                    Date = DateTime(year, month, day).ToString "yyyy.MM.dd"
                    Seats = sut.SeatingCapacity - reservationOnDay.Item.Quantity }
                |] }
        Assert.Equal(expected, actual)

module NotificationsControllerTests =
    [<Theory; TestConventions>]
    let SutIsController (sut : NotificationsController) =
        Assert.IsAssignableFrom<ApiController> sut

    [<Theory; TestConventions>]
    let NotificationsAreExposedForExpection
        ([<Frozen>]expected : Notifications.INotifications)
        (sut : NotificationsController) =

        let actual : Notifications.INotifications = sut.Notifications
        Assert.Equal<Notifications.INotifications>(expected, actual)

    [<Theory; TestConventions>]
    let GetWithoutMatchingNotificationReturnsCorrectResult
        (sut : NotificationsController)
        (id : Guid) =
        
        Assert.False(sut.Notifications |> Seq.exists (fun n -> n.Item.About = id))

        let response : HttpResponseMessage = sut.Get id
        let actual = response.Content.ReadAsAsync<NotificationListRendition>().Result

        Assert.Empty(actual.Notifications)

    [<Theory; TestConventions>]
    let GetWithMatchingNotificationReturnsCorrectResult
        (sut : NotificationsController) =

        let target = sut.Notifications |> Seq.toList |> PickRandom
        let id = target.Item.About

        let response = sut.Get id
        let actual = response.Content.ReadAsAsync<NotificationListRendition>().Result

        let expected = {
            NotificationRendition.About = target.Item.About.ToString()
            Type = target.Item.Type
            Message = target.Item.Message }
        Assert.Equal(
            { Notifications = [| expected |] },
            actual)

    [<Theory; TestConventions>]
    let GetReturnsCorrectStatusCode(sut : NotificationsController, id : Guid) =
        let response = sut.Get id
        Assert.True(
            response.IsSuccessStatusCode,
            sprintf "Actual status code: %O" response.StatusCode)
