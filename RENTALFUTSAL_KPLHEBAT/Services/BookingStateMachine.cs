using System;
using System.Collections.Generic;
using System.Text;
using RENTALFUTSAL_KPLHEBAT.Models;

namespace RENTALFUTSAL_KPLHEBAT.Services
{
    public sealed class BookingStateMachine
    {
        private static readonly Dictionary<BookingStatus, BookingStatus[]> ValidTransitions = new()
        {
            [BookingStatus.PendingPayment] = new[] { BookingStatus.Paid, BookingStatus.Cancelled },
            [BookingStatus.Paid] = new[] { BookingStatus.Completed, BookingStatus.Cancelled },
            [BookingStatus.Cancelled] = Array.Empty<BookingStatus>(),
            [BookingStatus.Completed] = Array.Empty<BookingStatus>()
        };

        public bool CanMoveTo(BookingStatus currentStatus, BookingStatus targetStatus)
        {
            return ValidTransitions.TryGetValue(currentStatus, out BookingStatus[]? targets)
                && targets.Contains(targetStatus);
        }
    }
}
