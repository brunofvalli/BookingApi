module Ploeh.Samples.Booking.HttpApi.BoundaryTests.Dsl

open System.Net.Http
open System.Threading.Tasks
open Newtonsoft.Json

type HttpContent with
    member this.ReadAsJsonAsync() =
        let readJson (t : Task<string>) =
            JsonConvert.DeserializeObject t.Result
        this.ReadAsStringAsync().ContinueWith(fun t -> readJson t)