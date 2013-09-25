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

[<Theory; BoundaryTestConventions>]
let GetMonthReturnsCorrectResponse(client : HttpClient,
                                   year : int) =
    let month = [1 .. 12] |> PickRandom
    
    let response = client.GetAsync(sprintf "availability/%i/%i" year month).Result
    let json = response.Content.ReadAsJsonAsync().Result
    
    let expectedDays = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
    Assert.True(
        response.IsSuccessStatusCode,
        sprintf "Actual status code: %O" response.StatusCode)
    Assert.Equal(expectedDays, json?openings |> Seq.length)

[<Theory; BoundaryTestConventions>]
let GetDayReturnsCorrectResponse(client : HttpClient,
                                 year : int) =
    let month = [1 .. 12] |> PickRandom
    let daysInMonth = CultureInfo.CurrentCulture.Calendar.GetDaysInMonth(year, month)
    let day = [1 .. daysInMonth] |> PickRandom

    let response = client.GetAsync(sprintf "availability/%i/%i/%i" year month day).Result
    let json = response.Content.ReadAsJsonAsync().Result
    
    Assert.True(
        response.IsSuccessStatusCode,
        sprintf "Actual status code: %O" response.StatusCode)
    Assert.Equal(1, json?openings |> Seq.length)