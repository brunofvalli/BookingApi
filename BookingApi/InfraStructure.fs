module Ploeh.Samples.Booking.HttpApi.InfraStructure

open System.Web.Http
open Ploeh.Samples.Booking.HttpApi

type HttpRouteDefaults = { Controller : string; Id : obj }

let ConfigureRoutes (config : HttpConfiguration) =
    config.Routes.MapHttpRoute(
        "DefaultAPI",
        "{controller}/{id}",
        { Controller = "Home"; Id = RouteParameter.Optional }) |> ignore

