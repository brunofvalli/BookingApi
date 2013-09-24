namespace Ploeh.Samples.Booking.HttpApi

open System

[<CLIMutable>]
type MakeReservationRendition = {
    Date : string
    Name : string
    Email : string
    Quantity : int }

[<CLIMutable>]
type OpeningsRendition = {
    Date : string
    Seats : int }

[<CLIMutable>]
type InventoryRendition = {
    Openings : OpeningsRendition array }
