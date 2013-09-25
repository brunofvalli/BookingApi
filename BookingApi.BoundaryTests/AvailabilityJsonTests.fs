module Ploeh.Samples.Booking.HttpApi.BoundaryTests.AvailabilityJsonTests

open System.Net.Http
open Xunit
open Xunit.Extensions

[<Theory; BoundaryTestConventions>]
let GetYearReturnsCorrectResponse(client : HttpClient,
                                  year : int) =
    let response = client.GetAsync(sprintf "availability/%i" year).Result

    Assert.True(
        response.IsSuccessStatusCode,
        sprintf "Actual status code: %O" response.StatusCode)