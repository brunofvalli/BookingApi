﻿namespace Ploeh.Samples.Booking.HttpApi

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
    
    let subject = new Subject<Envelope<MakeReservation>>()

    member this.Post (rendition : MakeReservationRendition) =
        let cmd =
            {
                MakeReservation.Date = DateTime.Parse rendition.Date
                Name = rendition.Name
                Email = rendition.Email
                Quantity = rendition.Quantity
            }
            |> EnvelopWithDefaults
        subject.OnNext cmd

        this.Request.CreateResponse(
            HttpStatusCode.Accepted,
            {
                Links =
                    [| {
                        Rel = "http://ploeh.samples/notification"
                        Href = "notifications/" + cmd.Id.ToString "N" } |] })

    interface IObservable<Envelope<MakeReservation>> with
        member this.Subscribe observer = subject.Subscribe observer

    override this.Dispose disposing =
        if disposing then subject.Dispose()
        base.Dispose disposing

type AvailabilityController(reservations : Reservations.IReservations,
                            seatingCapacity : int) =
    inherit ApiController()

    let getAvailableSeats map (now : DateTimeOffset) date =
        if date < now.Date then 0
        elif map |> Map.containsKey date then
            seatingCapacity - (map |> Map.find date)
        else seatingCapacity

    let toMapOfDatesAndQuantities (min, max) reservations =
        reservations
        |> Reservations.Between min max
        |> Seq.groupBy (fun r -> r.Item.Date)
        |> Seq.map (fun (d, rs) ->
            (d, rs |> Seq.map (fun r -> r.Item.Quantity) |> Seq.sum))
        |> Map.ofSeq

    let mapToOpening getAvailableSeats (d : DateTime) =
        {
            Date = d.ToString "yyyy.MM.dd"
            Seats = getAvailableSeats d
        }

    let getOpeniningsIn period =
        let boundaries = Dates.BoundariesIn period
        let map = reservations |> toMapOfDatesAndQuantities boundaries
        let getAvailable = getAvailableSeats map DateTimeOffset.Now
        let toOpening = mapToOpening getAvailable
        
        Dates.In period
        |> Seq.map toOpening
        |> Seq.toArray

    member this.Get year =
        let openings = getOpeniningsIn(Year(year))

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month) =
        let openings = getOpeniningsIn(Month(year, month))

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.Get(year, month, day) =
        let openings = getOpeniningsIn(Day(year, month, day))

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Openings = openings })

    member this.SeatingCapacity = seatingCapacity

type NotificationsController(notifications : Notifications.INotifications) =
    inherit ApiController()

    member this.Get id =
        let toRendition (n : Envelope<Notification>) = {
            About = n.Item.About.ToString()
            Type = n.Item.Type
            Message = n.Item.Message }
        let matches =
            notifications
            |> Notifications.About id
            |> Seq.map toRendition
            |> Seq.toArray

        this.Request.CreateResponse(
            HttpStatusCode.OK,
            { Notifications = matches })

    member this.Notifications = notifications
