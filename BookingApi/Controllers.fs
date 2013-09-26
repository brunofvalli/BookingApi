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
    member this.Post (rendition : MakeReservationRendition) =
        new HttpResponseMessage(HttpStatusCode.Accepted)

type AvailabilityController(seatingCapacity : int) =
    inherit ApiController()
    member this.Get year =
        let now = DateTimeOffset.Now
        let openings =
            Dates.InYear year
            |> Seq.map (fun d -> 
                {
                    Date = d.ToString("o")
                    Seats = if d < now.Date then 0 else seatingCapacity } )
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month) =
        let openings =
            Dates.InMonth year month
            |> Seq.map (fun d ->
                {
                    Date = d.ToString("o")
                    Seats = seatingCapacity } )
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month, day) =
        let opening = {
            Date = DateTime(year, month, day).ToString("o")
            Seats = seatingCapacity }
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = [| opening |] })

    member this.SeatingCapacity = seatingCapacity