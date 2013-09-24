﻿namespace Ploeh.Samples.Booking.HttpApi.UnitTests

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
    let GetUnreservedYearReturnsCorrectResult(sut : AvailabilityController,
                                              year : int) =
        let response : HttpResponseMessage = sut.Get year
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expectedRecords =
            Availability.DatesInYear year
            |> Seq.map (fun d ->
                {
                    Date = d.ToString("o")
                    Seats = sut.SeatingCapacity })
            |> Seq.toArray
        let expected = { Openings = expectedRecords }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetUnreservedMonthReturnsCorrectResult(sut : AvailabilityController,
                                               year : int) =
        let month = [1 .. 12] |> PickRandom

        let response : HttpResponseMessage = sut.Get(year, month)
        let actual = response.Content.ReadAsAsync<AvailabilityRendition>().Result

        let expectedOpenings =
            Availability.DatesInMonth year month
            |> Seq.map (fun d ->
                {
                    Date = d.ToString("o")
                    Seats = sut.SeatingCapacity })
            |> Seq.toArray
        let expected = { Openings = expectedOpenings }
        Assert.Equal(expected, actual)

    [<Theory; TestConventions>]
    let GetUnreservedDayReturnsCorrectResult(sut : AvailabilityController,
                                             year : int) =
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
