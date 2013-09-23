namespace Ploeh.Samples.Booking.HttpApi

open System

[<CLIMutable>]
type MakeReservationRendition = {
    Date : string
    Name : string
    Email : string
    Quantity : int }

[<CLIMutable>]
type InventoryRecordRendition = {
    Date : string
    Seats : int }

[<CLIMutable>]
type InventoryRendition = {
    Records : InventoryRecordRendition array }
