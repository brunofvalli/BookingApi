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
type AvailabilityRendition = {
    Openings : OpeningsRendition array }

[<CLIMutable>]
type NotificationRendition = {
    About : string
    Type : string
    Message : string }

[<CLIMutable>]
type NotificationListRendition = {
    Notifications : NotificationRendition array }

[<CLIMutable>]
type AtomLinkRendition = {
    Rel : string
    Href : string }

[<CLIMutable>]
type LinkListRendition = {
    Links : AtomLinkRendition array }
