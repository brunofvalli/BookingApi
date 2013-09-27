module Ploeh.Samples.Booking.HttpApi.BoundaryTests.ReservationRequestsJsonTests

open System
open System.Net
open System.Net.Http
open ImpromptuInterface.FSharp
open Xunit
open Xunit.Extensions

type ReservationRequestJson() = 
    [<DefaultValue>] val mutable date : string
    [<DefaultValue>] val mutable name : string
    [<DefaultValue>] val mutable email : string
    [<DefaultValue>] val mutable quantity : int

[<Theory; BoundaryTestConventions>]
let PostReservationRequestReturnsCorrectStatusCode(client : HttpClient,
                                                   date : DateTimeOffset,
                                                   name : string,
                                                   email : string,
                                                   quantity : int) =
    let json =
        ReservationRequestJson(
            date = date.ToString "o",
            name = name,
            email = email,
            quantity = quantity);
    let response = client.PostAsJsonAsync("reservations", json).Result
    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode)