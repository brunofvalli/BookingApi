namespace Ploeh.Samples.Booking.HttpApi

open System

[<CLIMutable>]
type MakeReservationCommand = {
    Date : DateTime
    Name : string
    Email : string
    Quantity : int }

[<AutoOpen>]
module Envelope =
    [<CLIMutable>]
    type Envelope<'T> = {
        Id : Guid
        Created : DateTimeOffset
        Item : 'T }

    let Envelop id created item = {
        Id = id
        Created = created
        Item = item }
