namespace Ploeh.Samples.Booking.HttpApi.UnitTests

open System.Net
open System.Net.Http
open System.Web.Http
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.UnitTests.Infrastructure
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

module InventoryControllerTests =
    [<Theory; TestConventions>]
    let SutIsController (sut : InventoryController) =
        Assert.IsAssignableFrom<ApiController> sut
