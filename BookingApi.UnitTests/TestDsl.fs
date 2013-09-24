module Ploeh.Samples.Booking.HttpApi.UnitTests.TestDsl

open System
open System.Net.Http
open System.Web.Http
open System.Web.Http.Hosting
open Ploeh.AutoFixture
open Ploeh.AutoFixture.AutoFoq
open Ploeh.AutoFixture.Kernel
open Ploeh.AutoFixture.Xunit

type WebApiCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Customize<HttpRequestMessage>(fun c ->
                c.Do(fun (x : HttpRequestMessage) ->
                    x.Properties.Add(
                        HttpPropertyKeys.HttpConfigurationKey,
                        new HttpConfiguration()))
                :> ISpecimenBuilder)

type TestConventions() =
    inherit CompositeCustomization(
        WebApiCustomization(),
        AutoFoqCustomization())

type TestConventionsAttribute() =
    inherit AutoDataAttribute(Fixture().Customize(TestConventions()))

let private r = Random()

let Shuffle source = source |> List.sortBy (fun _ -> r.Next())

let SelectRandom count source = source |> Shuffle |> Seq.take count

let PickRandom source = source |> SelectRandom 1 |> Seq.head