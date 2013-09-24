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

type InventoryController(seatingCapacity : int) =
    inherit ApiController()
    member this.Get year =
        let datesIn year =
            DateTime(year, 1, 1)
            |> Seq.unfold (fun d -> Some(d, d.AddDays 1.0))
            |> Seq.takeWhile (fun d -> d.Year <= year)

        let openings =
            datesIn year
            |> Seq.map (fun d -> 
                {
                    Date = d.ToString("o")
                    Seats = seatingCapacity } )
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.SeatingCapacity = seatingCapacity
