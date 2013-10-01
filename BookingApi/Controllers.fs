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

    let toMapOfDatesAndQuantities (min, max) reservations =
        reservations
        |> Reservations.Between min max
        |> Seq.groupBy (fun r -> r.Item.Date)
        |> Seq.map (fun (d, rs) ->
            (d, rs |> Seq.map (fun r -> r.Item.Quantity) |> Seq.sum))
        |> Map.ofSeq

    let toAvailabilityIn period reservations =
        let boundaries = Dates.BoundariesIn period
        let map = reservations |> toMapOfDatesAndQuantities boundaries
        getAvailableSeats map

    let mapToOpening getAvailableSeats (now : DateTimeOffset) (d : DateTime) =
        {
            Date = d.ToString "yyyy.MM.dd"
            Seats = if d < now.Date then 0 else getAvailableSeats d
        }

    member this.Get year =
        let getAvailable = reservations |> toAvailabilityIn(Year(year))
        let toOpening = DateTimeOffset.Now |> mapToOpening getAvailable
        let openings =
            Dates.In(Year(year))
            |> Seq.map toOpening
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month) =
        let getAvailable = reservations |> toAvailabilityIn(Month(year, month))
        let toOpening = DateTimeOffset.Now |> mapToOpening getAvailable
        let openings =
            Dates.In(Month(year, month))
            |> Seq.map toOpening
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month, day) =
        let getAvailable = reservations |> toAvailabilityIn(Day(year, month, day))
        let toOpening = DateTimeOffset.Now |> mapToOpening getAvailable
        let openings =
            Dates.In(Day(year, month, day))
            |> Seq.map toOpening
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.SeatingCapacity = seatingCapacity