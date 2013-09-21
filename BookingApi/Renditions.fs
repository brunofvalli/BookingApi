namespace Ploeh.Samples.Booking.HttpApi

open System

type MakeReservationRendition = {
    mutable Date : DateTimeOffset
    mutable Name : string
    mutable Email : string
    mutable Quantity : int }