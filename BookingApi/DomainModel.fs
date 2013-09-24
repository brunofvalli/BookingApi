namespace Ploeh.Samples.Booking.HttpApi

open System

module Dates =
    let InitInfinite (date : DateTime) =
        date |> Seq.unfold (fun d -> Some(d, d.AddDays 1.0))

    let InYear year =
        DateTime(year, 1, 1)
        |> InitInfinite
        |> Seq.takeWhile (fun d -> d.Year = year)

    let InMonth year month =
        DateTime(year, month, 1)
        |> InitInfinite
        |> Seq.takeWhile (fun d -> d.Month = month)