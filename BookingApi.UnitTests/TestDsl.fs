module Ploeh.Samples.Booking.HttpApi.UnitTests.TestDsl

open System
open System.Net.Http
open System.Web.Http
open System.Web.Http.Hosting
open System.Reflection
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

type DateStringCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Customizations.Add {
                new ISpecimenBuilder with
                    member this.Create(request, context) =
                        match request with
                        | :? ParameterInfo as pi
                            when pi.ParameterType = typeof<string>
                            && pi.Name.EndsWith("date", StringComparison.OrdinalIgnoreCase) ->
                            (context.Resolve typeof<DateTime> :?> DateTime).ToString "yyyy.MM.dd" :> obj
                        | :? PropertyInfo as prop
                            when prop.PropertyType = typeof<string>
                            && prop.Name.EndsWith("date", StringComparison.OrdinalIgnoreCase) ->
                            (context.Resolve typeof<DateTime> :?> DateTime).ToString "yyyy.MM.dd" :> obj
                        | _ -> NoSpecimen(request) :> obj }

type ReservationsCustomization() =
    interface ICustomization with
        member this.Customize fixture =
            fixture.Inject<Ploeh.Samples.Booking.HttpApi.Reservations.IReservations>(
                [] |> Ploeh.Samples.Booking.HttpApi.Reservations.ToReservations)

type TestConventions() =
    inherit CompositeCustomization(
        ReservationsCustomization(),
        DateStringCustomization(),
        WebApiCustomization(),
        AutoFoqCustomization())

type TestConventionsAttribute() =
    inherit AutoDataAttribute(Fixture().Customize(TestConventions()))

let private r = Random()

let Shuffle source = source |> List.sortBy (fun _ -> r.Next())

let SelectRandom count source = source |> Shuffle |> Seq.take count

let PickRandom source = source |> SelectRandom 1 |> Seq.head

type IFixture with
    member this.Generate<'T>() = this.Create<Generator<'T>>()