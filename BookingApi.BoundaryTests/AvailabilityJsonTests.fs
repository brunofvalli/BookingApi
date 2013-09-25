module Ploeh.Samples.Booking.HttpApi.BoundaryTests.AvailabilityJsonTests

open System.Net.Http
open System.Globalization
open ImpromptuInterface.FSharp
open Ploeh.Samples.Booking.HttpApi.BoundaryTests.Dsl
open Xunit
open Xunit.Extensions

[<Theory; BoundaryTestConventions>]
let GetYearReturnsCorrectResponse(client : HttpClient,
                                  year : int) =
    let response = client.GetAsync(sprintf "availability/%i" year).Result
    let json = response.Content.ReadAsJsonAsync().Result

    let expectedDays = CultureInfo.CurrentCulture.Calendar.GetDaysInYear year
    Assert.True(
        response.IsSuccessStatusCode,
        sprintf "Actual status code: %O" response.StatusCode)
    Assert.Equal(expectedDays, json?openings |> Seq.length)