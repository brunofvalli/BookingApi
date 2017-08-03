namespace Ploeh.Samples.Booking.HttpApi.BoundaryTests

open System.Net.Http
open Xunit
open Xunit.Extensions

module HomeJsonTests =
    [<Theory; BoundaryTestConventions>]
    let GetHomeReturnsCorrectStatusCode (client : HttpClient) =
        let response = client.GetAsync("").Result;
        Assert.True(
            response.IsSuccessStatusCode,
            sprintf "Actual status code: %O" response.StatusCode)