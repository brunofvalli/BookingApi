module Ploeh.Samples.Booking.HttpApi.UnitTests.Infrastructure

open Ploeh.AutoFixture
open Ploeh.AutoFixture.Kernel

let private factory =
    CompositeSpecimenBuilder(
        SeedIgnoringRelay(),
        MethodInvoker(ModestConstructorQuery()),
        TerminatingSpecimenBuilder())

let pool<'T> = Generator<'T>(factory) :> 'T seq