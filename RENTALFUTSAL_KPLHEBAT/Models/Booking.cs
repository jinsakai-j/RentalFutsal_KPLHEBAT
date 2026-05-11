using System;
using System.Collections.Generic;
using System.Text;

namespace RENTALFUTSAL_KPLHEBAT.Models
{
    public sealed class Booking
    {
        public int Id { get; set; }
        public int FieldId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public int DurationHours { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;

        public TimeOnly EndTime => StartTime.AddHours(DurationHours);

        public override string ToString()
        {
            return $"#{Id} | Lapangan {FieldId} | {CustomerName} | {Date:yyyy-MM-dd} {StartTime:HH:mm}-{EndTime:HH:mm} | {DurationHours} jam | Rp{TotalPrice:N0} | {Status}";
        }
    }
}
