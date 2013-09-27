namespace Ploeh.Samples.Booking.HttpApi

open System

[<CLIMutable>]
type MakeReservationCommand = {
    Date : DateTime
    Name : string
    Email : string
    Quantity : int }