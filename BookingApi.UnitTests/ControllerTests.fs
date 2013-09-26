namespace Ploeh.Samples.Booking.HttpApi.UnitTests

open System
open System.Net
open System.Net.Http
open System.Web.Http
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.UnitTests.TestDsl
open Xunit
open Xunit.Extensions

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
            Dates.InYear year
            |> Seq.map (fun d ->
                {
                    Date = d.ToString("o")
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
            Dates.InYear year
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "o"
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
            Dates.InYear year
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "o"
                    Seats = if d < now.Date then 0 else sut.SeatingCapacity })
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
            Dates.InMonth year month
            |> Seq.map (fun d ->
                {
                    Date = d.ToString("o")
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
            Dates.InMonth year month
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "o"
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
            Dates.InMonth year month
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "o"
                    Seats = if d < now.Date then 0 else sut.SeatingCapacity })
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
                    Date = DateTime(year, month, day).ToString("o")
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
                    Date = DateTime(year, month, day).ToString("o")
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
                    Date = DateTime(year, month, day).ToString("o")
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
                    Date = DateTime(year, month, day).ToString("o")
                    Seats = 0 }
                |] }
        Assert.Equal(expected, actual)
