namespace Ploeh.Samples.Booking.HttpApi.HttpHost

open System
open System.Reactive
open System.Web.Http
open FSharp.Reactive
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.Reservations
open Ploeh.Samples.Booking.HttpApi.InfraStructure

type HttpRouteDefaults = { Controller : string; Id : obj }

type Agent<'T> = Microsoft.FSharp.Control.MailboxProcessor<'T>

type Global() =
    inherit System.Web.HttpApplication()
    member this.Application_Start (sender : obj) (e : EventArgs) =
        let seatingCapacity = 10
        let reservations =
            System.Collections.Concurrent.ConcurrentBag<Envelope<Reservation>>()
        let notifications =
            System.Collections.Concurrent.ConcurrentBag<Envelope<Notification>>()

        let reservationSubject = new Subjects.Subject<Envelope<Reservation>>()
        reservationSubject.Subscribe reservations.Add |> ignore

        let notificationSubject = new Subjects.Subject<Notification>()
        notificationSubject
        |> Observable.map EnvelopWithDefaults
        |> Observable.subscribe notifications.Add ignore ignore
        |> ignore

        let agent = new Agent<Envelope<MakeReservation>>(fun inbox ->
            let rec loop () =
                async {
                    let! cmd = inbox.Receive()
                    let rs = reservations |> ToReservations
                    let handle = Handle seatingCapacity rs
                    let newReservations = handle cmd
                    match newReservations with
                    | Some(r) ->
                        reservationSubject.OnNext r
                        notificationSubject.OnNext
                            {
                                About = cmd.Id
                                Type = "Success"
                                Message =
                                    sprintf
                                        "Your reservation for %s was completed. We look forward to see you."
                                        (cmd.Item.Date.ToString "yyyy.MM.dd")
                            }
                    | _ ->
                        notificationSubject.OnNext
                            {
                                About = cmd.Id
                                Type = "Failure"
                                Message =
                                    sprintf
                                        "We regret to inform you that your reservation for %s could not be completed, because we are already fully booked."
                                        (cmd.Item.Date.ToString "yyyy.MM.dd")
                            }
                    return! loop() }
            loop())
        do agent.Start()

        Configure
            (reservations |> ToReservations)
            (Observer.Create agent.Post)
            (notifications |> Notifications.ToNotifications)
            seatingCapacity
            GlobalConfiguration.Configuration