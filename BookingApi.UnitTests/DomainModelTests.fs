namespace Ploeh.Samples.Booking.HttpApi.UnitTests

open System
open Ploeh.Samples.Booking.HttpApi.UnitTests.Infrastructure
open Xunit
open Xunit.Extensions

module AvailabilityTests =
    open Ploeh.Samples.Booking.HttpApi.Availability

    [<Theory; TestConventions>]
    let DatesFromReturnsCorrectSequence (dates : DateTime seq) =
        let sortedDates =
            dates |> Seq.map (fun d -> d.Date) |> Seq.sort |> Seq.toList
        let head = sortedDates |> List.head
        let last = sortedDates |> Seq.nth 1

        let actual : DateTime seq = DatesFrom head

        Assert.Equal(head, actual |> Seq.head)
        Assert.Equal(last.AddDays -1.0, actual
                                        |> Seq.takeWhile (fun d -> d <> last)
                                        |> Seq.toList
                                        |> List.rev
                                        |> List.head)