using System;
using System.Collections.Generic;
using System.Text;
using RENTALFUTSAL_KPLHEBAT.Models;

namespace RENTALFUTSAL_KPLHEBAT.Services
{
    internal class BookingService
    {
        private readonly JsonDataStore<Booking> _bookingStore;
        private readonly FieldService _fieldService;
        private readonly BookingStateMachine _stateMachine;

        public BookingService(JsonDataStore<Booking> bookingStore, FieldService fieldService, BookingStateMachine stateMachine)
        {
            _bookingStore = bookingStore ?? throw new ArgumentNullException(nameof(bookingStore));
            _fieldService = fieldService ?? throw new ArgumentNullException(nameof(fieldService));
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
        }

        public List<Booking> GetAllBookings()
        {
            return _bookingStore.Load()
                .OrderBy(booking => booking.Date)
                .ThenBy(booking => booking.StartTime)
                .ToList();
        }

        public List<Booking> GetBookingsByDate(DateOnly date)
        {
            return _bookingStore.Load()
                .Where(booking => booking.Date == date && booking.Status != BookingStatus.Cancelled)
                .OrderBy(booking => booking.FieldId)
                .ThenBy(booking => booking.StartTime)
                .ToList();
        }

        public OperationResult<Booking> CreateBooking(int fieldId, string customerName, string customerPhone, DateOnly date, TimeOnly startTime, int durationHours)
        {
            Field? field = _fieldService.GetById(fieldId);
            if (field is null)
            {
                return OperationResult<Booking>.Fail("Lapangan tidak ditemukan atau tidak aktif.");
            }

            if (string.IsNullOrWhiteSpace(customerName))
            {
                return OperationResult<Booking>.Fail("Nama penyewa tidak boleh kosong.");
            }

            if (string.IsNullOrWhiteSpace(customerPhone))
            {
                return OperationResult<Booking>.Fail("Nomor HP penyewa tidak boleh kosong.");
            }

            if (durationHours <= 0 || durationHours > 6)
            {
                return OperationResult<Booking>.Fail("Durasi booking harus 1 sampai 6 jam.");
            }

            if (startTime.Minute != 0)
            {
                return OperationResult<Booking>.Fail("Jam mulai harus tepat per jam, contoh 18:00.");
            }

            TimeOnly endTime = startTime.AddHours(durationHours);
            if (startTime < new TimeOnly(8, 0) || endTime > new TimeOnly(23, 0))
            {
                return OperationResult<Booking>.Fail("Jam booking hanya boleh dari 08:00 sampai 23:00.");
            }

            List<Booking> bookings = _bookingStore.Load();
            bool isConflict = bookings.Any(existing =>
                existing.FieldId == fieldId &&
                existing.Date == date &&
                existing.Status != BookingStatus.Cancelled &&
                IsTimeOverlap(startTime, endTime, existing.StartTime, existing.EndTime));

            if (isConflict)
            {
                return OperationResult<Booking>.Fail("Jadwal bentrok. Pilih jam atau lapangan lain.");
            }

            int nextId = bookings.Count == 0 ? 1 : bookings.Max(booking => booking.Id) + 1;
            Booking newBooking = new()
            {
                Id = nextId,
                FieldId = fieldId,
                CustomerName = customerName.Trim(),
                CustomerPhone = customerPhone.Trim(),
                Date = date,
                StartTime = startTime,
                DurationHours = durationHours,
                TotalPrice = field.PricePerHour * durationHours,
                Status = BookingStatus.PendingPayment
            };

            bookings.Add(newBooking);
            _bookingStore.Save(bookings);

            return OperationResult<Booking>.Ok(newBooking, "Booking berhasil dibuat.");
        }

        public OperationResult<Booking> PayBooking(int bookingId, decimal amountPaid)
        {
            if (bookingId <= 0)
            {
                return OperationResult<Booking>.Fail("ID booking tidak valid.");
            }

            List<Booking> bookings = _bookingStore.Load();
            Booking? booking = bookings.FirstOrDefault(item => item.Id == bookingId);
            if (booking is null)
            {
                return OperationResult<Booking>.Fail("Booking tidak ditemukan.");
            }

            if (!_stateMachine.CanMoveTo(booking.Status, BookingStatus.Paid))
            {
                return OperationResult<Booking>.Fail($"Booking dengan status {booking.Status} tidak bisa dibayar.");
            }

            if (amountPaid < booking.TotalPrice)
            {
                return OperationResult<Booking>.Fail("Jumlah pembayaran kurang dari total harga.");
            }

            booking.Status = BookingStatus.Paid;
            _bookingStore.Save(bookings);

            return OperationResult<Booking>.Ok(booking, "Pembayaran berhasil.");
        }

        public OperationResult<Booking> CancelBooking(int bookingId)
        {
            if (bookingId <= 0)
            {
                return OperationResult<Booking>.Fail("ID booking tidak valid.");
            }

            List<Booking> bookings = _bookingStore.Load();
            Booking? booking = bookings.FirstOrDefault(item => item.Id == bookingId);
            if (booking is null)
            {
                return OperationResult<Booking>.Fail("Booking tidak ditemukan.");
            }

            if (!_stateMachine.CanMoveTo(booking.Status, BookingStatus.Cancelled))
            {
                return OperationResult<Booking>.Fail($"Booking dengan status {booking.Status} tidak bisa dibatalkan.");
            }

            booking.Status = BookingStatus.Cancelled;
            _bookingStore.Save(bookings);

            return OperationResult<Booking>.Ok(booking, "Booking berhasil dibatalkan.");
        }

        public static bool IsTimeOverlap(TimeOnly startA, TimeOnly endA, TimeOnly startB, TimeOnly endB)
        {
            return startA < endB && startB < endA;
        }
    }
}
