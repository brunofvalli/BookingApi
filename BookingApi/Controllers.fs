namespace Ploeh.Samples.Booking.HttpApi

open System
open System.Net
open System.Net.Http
open System.Web.Http

type HomeController() =
    inherit ApiController()
    member this.Get() = new HttpResponseMessage()

type ReservationsController() =
    inherit ApiController()
    member this.Post rendition =
        new HttpResponseMessage(HttpStatusCode.Accepted)

type AvailabilityController(seatingCapacity : int) =
    inherit ApiController()
    member this.Get year =
        let openings =
            Availability.DatesInYear year
            |> Seq.map (fun d -> 
                {
                    Date = d.ToString("o")
                    Seats = seatingCapacity } )
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month) =
        let openings =
            Availability.DatesInMonth year month
            |> Seq.map (fun d ->
                {
                    Date = d.ToString("o")
                    Seats = seatingCapacity } )
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.SeatingCapacity = seatingCapacity