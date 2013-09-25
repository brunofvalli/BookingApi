module Ploeh.Samples.Booking.HttpApi.InfraStructure

open System
open System.Web.Http
open System.Web.Http.Dispatcher
open System.Web.Http.Controllers
open Ploeh.Samples.Booking.HttpApi

type CompositionRoot() =
    interface IHttpControllerActivator with
        member this.Create(request, controllerDescriptor, controllerType) =
            if controllerType = typeof<HomeController> then
                new HomeController() :> IHttpController
            elif controllerType = typeof<ReservationsController> then
                new ReservationsController() :> IHttpController
            elif controllerType = typeof<AvailabilityController> then
                new AvailabilityController(10) :> IHttpController
            else
                raise
                <| ArgumentException(
                    sprintf "Unknown controller type requested: %O" controllerType,
                    "controllerType")

let ConfigureServices (config : HttpConfiguration) =
    config.Services.Replace(
        typeof<IHttpControllerActivator>,
        CompositionRoot())

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

let Configure config =
    ConfigureRoutes config
    ConfigureServices config
    ConfigureFormatting config
