namespace Ploeh.Samples.Booking.HttpApi

open System

type MakeReservationRendition() =
    [<DefaultValue>] val mutable Date : string
    [<DefaultValue>] val mutable Name : string
    [<DefaultValue>] val mutable Email : string
    [<DefaultValue>] val mutable Quantity : int
