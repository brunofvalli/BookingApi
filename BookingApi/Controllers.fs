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

    let toReservationMap (min, max) reservations =
        reservations
        |> Reservations.Between min max
        |> Seq.groupBy (fun r -> r.Item.Date)
        |> Seq.map (fun (d, rs) ->
            (d, rs |> Seq.map (fun r -> r.Item.Quantity) |> Seq.sum))
        |> Map.ofSeq

    let toAvailabilityIn period reservations =
        let boundaries = Dates.BoundariesIn period
        let map = reservations |> toReservationMap boundaries
        getAvailableSeats map

    member this.Get year =
        let getAvailable =
            reservations
            |> toAvailabilityIn(Year(year))

        let now = DateTimeOffset.Now
        let openings =
            Dates.In(Year(year))
            |> Seq.map (fun d -> 
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = if d < now.Date then 0 else getAvailable d } )
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month) =
        let getAvailable =
            reservations
            |> toAvailabilityIn(Month(year, month))

        let now = DateTimeOffset.Now
        let openings =
            Dates.In(Month(year, month))
            |> Seq.map (fun d ->
                {
                    Date = d.ToString "yyyy.MM.dd"
                    Seats = if d < now.Date then 0 else getAvailable d } )
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month, day) =
        let reservedSeats =
            reservations
            |> Reservations.On (DateTime(year, month, day))
            |> Seq.sumBy (fun r -> r.Item.Quantity)

        let now = DateTimeOffset.Now
        let requestedDate = DateTimeOffset(DateTime(year, month, day), now.Offset)
        let opening = {
            Date = DateTime(year, month, day).ToString "yyyy.MM.dd"
            Seats = if requestedDate.Date < now.Date
                    then 0
                    else seatingCapacity - reservedSeats }
        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = [| opening |] })

    member this.SeatingCapacity = seatingCapacity