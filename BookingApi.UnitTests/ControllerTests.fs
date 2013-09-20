module Ploeh.Samples.Booking.HttpApi.UnitTests.ControllerTests

open System.Web.Http
open Ploeh.Samples.Booking.HttpApi.Controllers
open Ploeh.Samples.Booking.HttpApi.UnitTests.Infrastructure
open Xunit

[<Fact>]
let SutIsController() =
    let sut = inSetOf<HomeController> |> Seq.head
    Assert.IsAssignableFrom<ApiController>(sut)