namespace Ploeh.Samples.Booking.HttpApi.HttpHost

open System
open System.Web.Http
open Ploeh.Samples.Booking.HttpApi.InfraStructure

type HttpRouteDefaults = { Controller : string; Id : obj }

type Global() =
    inherit System.Web.HttpApplication()
    member this.Application_Start (sender : obj) (e : EventArgs) =
        ConfigureRoutes GlobalConfiguration.Configuration