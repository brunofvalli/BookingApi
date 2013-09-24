namespace Ploeh.Samples.Booking.HttpApi

open System

module Availability =
    let DatesFrom (date : DateTime) =
        date |> Seq.unfold (fun d -> Some(d, d.AddDays 1.0))

