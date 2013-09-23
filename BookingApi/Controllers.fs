namespace Ploeh.Samples.Booking.HttpApi

open System.Net
open System.Net.Http
open System.Web.Http

type HomeController() =
    inherit ApiController()
    member this.Get() = new HttpResponseMessage()

type ReservationsController() =
    inherit ApiController()
    member this.Post rendition =
        new HttpResponseMessage(HttpStatusCode.Accepted)

type InventoryController() =
    inherit ApiController()
