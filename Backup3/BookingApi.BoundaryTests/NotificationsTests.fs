module Ploeh.Samples.Booking.HttpApi.BoundaryTests.NotificationsTests

open System
open System.Net.Http
open ImpromptuInterface.FSharp
open Ploeh.Samples.Booking.HttpApi.BoundaryTests.Dsl
open Xunit
open Xunit.Extensions

[<Theory; BoundaryTestConventions>]
let GetReturnsCorrectResponse(client : HttpClient, id : Guid) =
    let response = client.GetAsync("notifications/" + id.ToString "N").Result
    let json = response.Content.ReadAsJsonAsync().Result

    Assert.True(
        response.IsSuccessStatusCode,
        sprintf "Actual status code: %O" response.StatusCode)
    Assert.NotNull(json?notifications)