namespace Ploeh.Samples.Booking.HttpApi.UnitTests

open System.Net.Http
open System.Web.Http
open Ploeh.Samples.Booking.HttpApi.Controllers
open Ploeh.Samples.Booking.HttpApi.UnitTests.Infrastructure
open Xunit

module HomeControllerTests =
    [<Fact>]
    let SutIsController() =
        let sut = pool<HomeController> |> Seq.head
        Assert.IsAssignableFrom<ApiController>(sut)

    [<Fact>]
    let GetReturnsCorrectResult() =
        let sut = pool<HomeController> |> Seq.head
    
        let actual : HttpResponseMessage = sut.Get()
    
        Assert.True(
            actual.IsSuccessStatusCode,
            "Actual status code: " + actual.StatusCode.ToString())

module ReservationRequestsControllerTests =
    [<Fact>]
    let SutIsController() =
        let sut = pool<ReservationsController> |> Seq.head
        Assert.IsAssignableFrom<ApiController>(sut)