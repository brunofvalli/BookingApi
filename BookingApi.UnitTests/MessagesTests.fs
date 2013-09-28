module Ploeh.Samples.Booking.HttpApi.UnitTests.MessagesTests

open System
open Ploeh.Samples.Booking.HttpApi
open Ploeh.Samples.Booking.HttpApi.UnitTests.TestDsl
open Xunit
open Xunit.Extensions

[<Theory; TestConventions>]
let EnvelopReturnsCorrectResult(item : obj,
                                id : Guid,
                                created : DateTimeOffset) =
    let actual = Envelop id created item

    let expected = { Id = id; Created = created; Item = item }
    Assert.Equal(expected, actual)