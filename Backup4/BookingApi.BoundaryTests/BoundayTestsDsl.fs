module Ploeh.Samples.Booking.HttpApi.BoundaryTests.Dsl

open System
open System.Net.Http
open System.Threading.Tasks
open Newtonsoft.Json

type HttpContent with
    member this.ReadAsJsonAsync() =
        let readJson (t : Task<string>) =
            JsonConvert.DeserializeObject t.Result
        this.ReadAsStringAsync().ContinueWith(fun t -> readJson t)

let private r = Random()

let Shuffle source = source |> List.sortBy (fun _ -> r.Next())

let SelectRandom count source = source |> Shuffle |> Seq.take count

let PickRandom source = source |> SelectRandom 1 |> Seq.head