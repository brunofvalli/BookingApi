namespace Ploeh.Samples.Booking.HttpApi

open System

module Availability =
    let DatesFrom (date : DateTime) =
        date |> Seq.unfold (fun d -> Some(d, d.AddDays 1.0))

    let DatesInYear year =
        DateTime(year, 1, 1)
        |> DatesFrom
        |> Seq.takeWhile (fun d -> d.Year = year)

