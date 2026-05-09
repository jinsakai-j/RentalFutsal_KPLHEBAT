using System;
using System.Collections.Generic;
using System.Text;

namespace RENTALFUTSAL_KPLHEBAT.Models
{
    public sealed class TimeSlot
    {
        public TimeSlot(DateTime startTime, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "Durasi harus lebih dari 0.");
            }

            StartTime = startTime;
            Duration = duration;
        }

        public DateTime StartTime { get; }
        public TimeSpan Duration { get; }
        public DateTime EndTime => StartTime.Add(Duration);

        public bool Overlaps(TimeSlot other)
        {
            ArgumentNullException.ThrowIfNull(other);
            return StartTime < other.EndTime && other.StartTime < EndTime;
        }
    }
}
