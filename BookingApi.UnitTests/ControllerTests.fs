namespace Ploeh.Samples.Booking.HttpApi.UnitTests

open System.Net
open System.Net.Http
open System.Web.Http
open Ploeh.Samples.Booking.HttpApi
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

    [<Fact>]
    let PostReturnsCorrectResult() =
        let sut = pool<ReservationsController> |> Seq.head
        let rendition = pool<MakeReservationRendition> |> Seq.head
        
        let actual : HttpResponseMessage = sut.Post rendition

        Assert.Equal(HttpStatusCode.Accepted, actual.StatusCode)
