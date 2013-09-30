namespace Ploeh.Samples.Booking.HttpApi

open System
open System.Net
open System.Net.Http
open System.Web.Http
open System.Reactive.Subjects

type HomeController() =
    inherit ApiController()
    member this.Get() = new HttpResponseMessage()

type ReservationsController() =
    inherit ApiController()
    let subject = new Subject<MakeReservation>()
    member this.Post (rendition : MakeReservationRendition) =
        subject.OnNext {
            Date = DateTime.Parse rendition.Date
            Name = rendition.Name
            Email = rendition.Email
            Quantity = rendition.Quantity }
        new HttpResponseMessage(HttpStatusCode.Accepted)
    interface IObservable<MakeReservation> with
        member this.Subscribe observer = subject.Subscribe observer

type AvailabilityController(reservations : Reservations.IReservations,
                            seatingCapacity : int) =
    inherit ApiController()

    let getAvailableSeats map date =
        if map |> Map.containsKey date then
            seatingCapacity - (map |> Map.find date)
        else seatingCapacity

    member this.Get year =
        let firstTickOfYear = DateTime(year, 1, 1)
        let lastTickofYear = DateTime(year + 1, 1, 1).AddTicks -1L
        let map =
            reservations
            |> Reservations.Between firstTickOfYear lastTickofYear
            |> Seq.groupBy (fun r -> r.Item.Date)
            |> Seq.map (fun (d, rs) ->
                (d, rs |> Seq.map (fun r -> r.Item.Quantity) |> Seq.sum))
            |> Map.ofSeq

        let now = DateTimeOffset.Now
        let openings =
            Dates.InYear year
            |> Seq.map (fun d -> 
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = if d < now.Date then 0 else getAvailableSeats map d } )
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month) =
        let now = DateTimeOffset.Now
        let openings =
            Dates.InMonth year month
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = if d < now.Date then 0 else seatingCapacity } )
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month, day) =
        let now = DateTimeOffset.Now
        let requestedDate = DateTimeOffset(DateTime(year, month, day), now.Offset)
        let opening = {
            Date = DateTime(year, month, day).ToString "yyyy.MM.dd"
            Seats = if requestedDate.Date < now.Date then 0 else seatingCapacity }
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = [| opening |] })

    member this.SeatingCapacity = seatingCapacity