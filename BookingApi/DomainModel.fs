namespace Ploeh.Samples.Booking.HttpApi

open System

type Period =
    | Year of int
    | Month of int * int

module Dates =
    let InitInfinite (date : DateTime) =
        date |> Seq.unfold (fun d -> Some(d, d.AddDays 1.0))

    let In period =
        let generate dt predicate =
            dt |> InitInfinite |> Seq.takeWhile predicate
        match period with
        | Year(y) -> generate (DateTime(y, 1, 1)) (fun d -> d.Year = y)
        | Month(y, m) -> generate (DateTime(y, m, 1)) (fun d -> d.Month = m)

module Reservations =

    type IReservations =
        inherit seq<Envelope<Reservation>>
        abstract Between : DateTime -> DateTime -> seq<Envelope<Reservation>>

    type ReservationsInMemory(reservations) =
        new() = ReservationsInMemory(Seq.empty<Envelope<Reservation>>)
        interface IReservations with
            member this.Between min max =
                reservations
                |> Seq.filter (fun r -> min <= r.Item.Date && r.Item.Date <= max)
            member this.GetEnumerator() =
                reservations.GetEnumerator()
            member this.GetEnumerator() =
                (this :> seq<Envelope<Reservation>>).GetEnumerator() :> System.Collections.IEnumerator

    let ToReservations reservations = ReservationsInMemory(reservations)

    let Between min max (reservations : IReservations) =
        reservations.Between min max

    let On (date : DateTime) reservations =
        let min = date.Date
        let max = (min.AddDays 1.0) - TimeSpan.FromTicks 1L
        reservations |> Between min max

    let Handle capacity reservations (request : Envelope<MakeReservation>) =
        let reservedSeatsOnDate =
            reservations
            |> On request.Item.Date
            |> Seq.sumBy (fun r -> r.Item.Quantity)
        if capacity - reservedSeatsOnDate < request.Item.Quantity then
            None
        else
            {
                Date = request.Item.Date
                Name = request.Item.Name
                Email = request.Item.Email
                Quantity = request.Item.Quantity
            }
            |> EnvelopWithDefaults
            |> Some
