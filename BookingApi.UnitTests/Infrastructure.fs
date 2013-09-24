module Ploeh.Samples.Booking.HttpApi.UnitTests.Infrastructure

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