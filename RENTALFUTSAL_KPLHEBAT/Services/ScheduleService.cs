using RENTALFUTSAL_KPLHEBAT.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RENTALFUTSAL_KPLHEBAT.Services
{
    public sealed class ScheduleService
    {
        private readonly JsonDataStore<Booking> _bookingStore;

        public ScheduleService(JsonDataStore<Booking> bookingStore)
        {
            _bookingStore = bookingStore ?? throw new ArgumentNullException(nameof(bookingStore));
        }

        public OperationResult<bool> IsAvailable(int fieldId, DateTime startTime, TimeSpan duration)
        {
            if (fieldId <= 0)
            {
                return OperationResult<bool>.Fail("ID lapangan tidak valid.");
            }

            if (duration <= TimeSpan.Zero)
            {
                return OperationResult<bool>.Fail("Durasi booking harus lebih dari 0.");
            }

            TimeSlot requestedSlot = new(startTime, duration);
            bool hasConflict = _bookingStore.Load()
                .Where(booking => booking.FieldId == fieldId && booking.Status != BookingStatus.Cancelled)
                .Any(booking => GetBookingSlot(booking).Overlaps(requestedSlot));

            return hasConflict
                ? OperationResult<bool>.Ok(false, "Jadwal bentrok. Pilih jam atau lapangan lain.")
                : OperationResult<bool>.Ok(true, "Jadwal tersedia.");
        }

        public OperationResult<bool> IsAvailable(int fieldId, DateOnly date, TimeOnly startTime, int durationHours)
        {
            if (durationHours <= 0)
            {
                return OperationResult<bool>.Fail("Durasi booking harus lebih dari 0.");
            }

            DateTime slotStart = date.ToDateTime(startTime);
            TimeSpan duration = TimeSpan.FromHours(durationHours);

            return IsAvailable(fieldId, slotStart, duration);
        }

        private static TimeSlot GetBookingSlot(Booking booking)
        {
            DateTime startTime = booking.Date.ToDateTime(booking.StartTime);
            TimeSpan duration = TimeSpan.FromHours(booking.DurationHours);

            return new TimeSlot(startTime, duration);
        }
    }
}
