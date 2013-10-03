namespace Ploeh.Samples.Booking.HttpApi.BoundaryTests

open System
open System.Net.Http
open System.Web.Http.SelfHost
open Ploeh.AutoFixture
open Ploeh.AutoFixture.Xunit
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.InfraStructure

type HttpClientCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            let configure config =
                Configure
                    ([] |> Reservations.ToReservations)
                    (System.Reactive.Observer.Create (fun _ -> ()))
                    ([] |> Notifications.ToNotifications)
                    10
                    config

            let createHttpClient() =
                let baseUri = fixture.Create<Uri>()
                let config = new HttpSelfHostConfiguration(baseUri)
                do  configure config
                let server = new HttpSelfHostServer(config)
                let client = new HttpClient(server)
                client.BaseAddress <- baseUri
                client
            fixture.Register<HttpClient>(fun () -> createHttpClient())

type BoundaryTestConventions() =
    inherit CompositeCustomization(
        HttpClientCustomization())

type BoundaryTestConventionsAttribute() =
    inherit AutoDataAttribute(Fixture().Customize(new BoundaryTestConventions()))