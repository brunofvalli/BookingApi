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

module Reserverations =

    type IReservations =
        inherit seq<Envelope<Reservation>>
        abstract Between : DateTime -> DateTime -> seq<Envelope<Reservation>>

    type InMemory(reservations) =
        new() = InMemory(Seq.empty<Envelope<Reservation>>)
        interface IReservations with
            member this.Between min max =
                reservations
                |> Seq.filter (fun r -> min <= r.Item.Date && r.Item.Date <= max)
            member this.GetEnumerator() =
                reservations.GetEnumerator()
            member this.GetEnumerator() =
                (this :> seq<Envelope<Reservation>>).GetEnumerator() :> System.Collections.IEnumerator

    let ToReservations reservations = InMemory(reservations)

    let Between min max (reservations : IReservations) =
        reservations.Between min max

    let On date reservations =
        let min = date.Date
        let max = (min.AddDays 1.0) - TimeSpan.FromTicks 1L
        reservations |> Between min max