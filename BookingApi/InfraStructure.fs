module Ploeh.Samples.Booking.HttpApi.InfraStructure

open System
open System.Net.Http
open System.Web.Http
open System.Web.Http.Dispatcher
open System.Web.Http.Controllers
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.Reservations

let Subscribe observer (observable : IObservable<'T>) = observable.Subscribe observer

type CompositionRoot(reservations : System.Collections.Concurrent.ConcurrentBag<Envelope<Reservation>>,
                     reservationRequestObserver,
                     seatingCapacity) =

    interface IHttpControllerActivator with
        member this.Create(request, controllerDescriptor, controllerType) =
            if controllerType = typeof<HomeController> then
                new HomeController() :> IHttpController
            elif controllerType = typeof<ReservationsController> then
                let c = new ReservationsController()
                c
                |> Observable.map EnvelopWithDefaults
                |> Subscribe reservationRequestObserver
                |> request.RegisterForDispose
                c :> IHttpController
            elif controllerType = typeof<AvailabilityController> then
                new AvailabilityController(
                    reservations |> ToReservations,
                    seatingCapacity) :> IHttpController
            else
                raise
                <| ArgumentException(
                    sprintf "Unknown controller type requested: %O" controllerType,
                    "controllerType")

let ConfigureServices reservations reservationRequestObserver seatingCapacity (config : HttpConfiguration) =
    config.Services.Replace(
        typeof<IHttpControllerActivator>,
        CompositionRoot(reservations, reservationRequestObserver, seatingCapacity))

type HttpRouteDefaults = { Controller : string; Id : obj }

let ConfigureRoutes (config : HttpConfiguration) =
    config.Routes.MapHttpRoute(
        "AvailabilityYear",
        "{controller}/{year}",
        { Controller = "Availability"; Id = RouteParameter.Optional }) |> ignore

    config.Routes.MapHttpRoute(
        "AvailabilityMonth",
        "{controller}/{year}/{month}",
        { Controller = "Availability"; Id = RouteParameter.Optional }) |> ignore

    config.Routes.MapHttpRoute(
        "AvailabilityDay",
        "{controller}/{year}/{month}/{day}",
        { Controller = "Availability"; Id = RouteParameter.Optional }) |> ignore

    config.Routes.MapHttpRoute(
        "DefaultAPI",
        "{controller}/{id}",
        { Controller = "Home"; Id = RouteParameter.Optional }) |> ignore

let ConfigureFormatting (config : HttpConfiguration) =
    config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <-
        Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

let Configure reservations reservationRequestObserver seatingCapacity config =
    ConfigureRoutes config
    ConfigureServices reservations reservationRequestObserver seatingCapacity config
    ConfigureFormatting config
