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
    let subject = new Subject<MakeReservationCommand>()
    member this.Post (rendition : MakeReservationRendition) =
        subject.OnNext {
            Date = DateTime.Parse rendition.Date
            Name = rendition.Name
            Email = rendition.Email
            Quantity = rendition.Quantity }
        new HttpResponseMessage(HttpStatusCode.Accepted)
    interface IObservable<MakeReservationCommand> with
        member this.Subscribe observer = subject.Subscribe observer

type AvailabilityController(seatingCapacity : int) =
    inherit ApiController()
    member this.Get year =
        let now = DateTimeOffset.Now
        let openings =
            Dates.InYear year
            |> Seq.map (fun d -> 
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = if d < now.Date then 0 else seatingCapacity } )
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