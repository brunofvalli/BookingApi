﻿module Ploeh.Samples.Booking.HttpApi.InfraStructure

open System
open System.Net.Http
open System.Web.Http
open System.Web.Http.Dispatcher
open FSharp.Reactive
open System.Web.Http.Controllers
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.Reservations

type CompositionRoot(reservations : IReservations,
                     reservationRequestObserver,
                     notifications,
                     seatingCapacity) =

    interface IHttpControllerActivator with
        member this.Create(request, controllerDescriptor, controllerType) =
            if controllerType = typeof<HomeController> then
                new HomeController() :> IHttpController
            elif controllerType = typeof<ReservationsController> then
                let c = new ReservationsController()
                c
                |> Observable.subscribeObserver reservationRequestObserver
                |> request.RegisterForDispose
                c :> IHttpController
            elif controllerType = typeof<AvailabilityController> then
                new AvailabilityController(
                    reservations,
                    seatingCapacity) :> IHttpController
            elif controllerType = typeof<NotificationsController> then
                new NotificationsController(notifications) :> IHttpController
            else
                raise
                <| ArgumentException(
                    sprintf "Unknown controller type requested: %O" controllerType,
                    "controllerType")

let ConfigureServices reservations reservationRequestObserver notifications seatingCapacity (config : HttpConfiguration) =
    config.Services.Replace(
        typeof<IHttpControllerActivator>,
        CompositionRoot(reservations, reservationRequestObserver, notifications, seatingCapacity))

type HttpRouteDefaults = { Controller : string; Id : obj }

let ConfigureRoutes (config : HttpConfiguration) =
    config.Routes.MapHttpRoute(
        "AvailabilityYear",
        "availability/{year}",
        { Controller = "Availability"; Id = RouteParameter.Optional }) |> ignore

    config.Routes.MapHttpRoute(
        "AvailabilityMonth",
        "availability/{year}/{month}",
        { Controller = "Availability"; Id = RouteParameter.Optional }) |> ignore

    config.Routes.MapHttpRoute(
        "AvailabilityDay",
        "availability/{year}/{month}/{day}",
        { Controller = "Availability"; Id = RouteParameter.Optional }) |> ignore

    config.Routes.MapHttpRoute(
        "DefaultAPI",
        "{controller}/{id}",
        { Controller = "Home"; Id = RouteParameter.Optional }) |> ignore

let ConfigureFormatting (config : HttpConfiguration) =
    config.Formatters.JsonFormatter.SerializerSettings.ContractResolver <-
        Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()

let Configure reservations reservationRequestObserver notifications seatingCapacity config =
    ConfigureRoutes config
    ConfigureServices reservations reservationRequestObserver notifications seatingCapacity config
    ConfigureFormatting config