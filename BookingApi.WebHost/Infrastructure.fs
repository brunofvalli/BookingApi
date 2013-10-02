namespace Ploeh.Samples.Booking.HttpApi.HttpHost

open System
open System.Reactive
open System.Web.Http
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

        let reservationEvents = new Subjects.Subject<Envelope<Reservation>>()
        reservationEvents.Subscribe (fun r -> reservations.Add r) |> ignore

        let agent = new Agent<Envelope<MakeReservation>>(fun inbox ->
            let rec loop () =
                async {
                    let! cmd = inbox.Receive()
                    let rs = reservations |> ToReservations
                    let handle = Handle seatingCapacity rs
                    let newReservations = handle cmd
                    match newReservations with
                    | Some(r) -> reservationEvents.OnNext r
                    | _ -> ()
                    return! loop() }
            loop())
        do agent.Start()
        let agentAsObserver = Observer.Create (fun cmd -> agent.Post cmd)

        Configure
            (reservations |> ToReservations)
            agentAsObserver
            seatingCapacity
            GlobalConfiguration.Configuration