﻿module Ploeh.Samples.Booking.HttpApi.UnitTests.Infrastructure

open System
open System.Net.Http
open System.Web.Http
open System.Web.Http.Hosting
open Ploeh.AutoFixture
open Ploeh.AutoFixture.AutoFoq
open Ploeh.AutoFixture.Kernel
open Ploeh.AutoFixture.Xunit

let private random = Random()

let private nextRandomInt64() =
    let buffer = Array.zeroCreate sizeof<int64>
    random.NextBytes buffer
    BitConverter.ToInt64(buffer, 0)

type private RandomInt32Generator() =
    interface ISpecimenBuilder with
        member this.Create(request, context) =
            match request with
            | :? Type as t when t = typeof<int> -> random.Next() :> obj
            | _ -> NoSpecimen(request) :> obj

type private RandomInt64Generator() =
    interface ISpecimenBuilder with
        member this.Create(request, context) =
            match request with
            | :? Type as t when t = typeof<int64> -> 
                nextRandomInt64() :> obj
            | _ -> NoSpecimen(request) :> obj

type private DateTimeGenerator() =
    interface ISpecimenBuilder with
        member this.Create(request, context) =
            match request with
            | :? Type as t when t = typeof<DateTime> ->
                let ticks =
                    Seq.initInfinite (fun _ -> nextRandomInt64())
                    |> Seq.where (fun x ->
                        DateTime.MinValue.Ticks <= x && x <= DateTime.MaxValue.Ticks)
                    |> Seq.head
                DateTime(ticks) :> obj
            | _ -> NoSpecimen(request) :> obj

let private factory =
    CompositeSpecimenBuilder(
        StringGenerator(fun () -> Guid.NewGuid() :> obj),
        RandomInt32Generator(),
        RandomInt64Generator(),
        DateTimeGenerator(),
        ParameterRequestRelay(),
        FiniteSequenceRelay(),
        SeedIgnoringRelay(),
        MethodInvoker(ModestConstructorQuery()),
        MultipleRelay(),
        EnumerableRelay(),
        TerminatingSpecimenBuilder())

let Pool<'T> = Generator<'T>(factory) :> 'T seq

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