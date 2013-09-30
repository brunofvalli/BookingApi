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
            Some({
                    Id = Guid.NewGuid()
                    Created = DateTimeOffset.Now
                    Item = {
                            Date = request.Item.Date
                            Name = request.Item.Name
                            Email = request.Item.Email
                            Quantity = request.Item.Quantity } })