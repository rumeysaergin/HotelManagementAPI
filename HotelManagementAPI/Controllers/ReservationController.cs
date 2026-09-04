using HotelManagementAPI.Data;
using HotelManagementAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly HotelManagementDbContext _context;

        public ReservationController(HotelManagementDbContext context)
        {
            _context = context;
        }

        // GET: api/Reservation
        [HttpGet]
        public IActionResult GetReservations()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var reservations = _context.Reservations
                .Where(x =>
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted)
                .ToList();

            return Ok(reservations);
        }

        // POST: api/Reservation
        [HttpPost]
        public IActionResult CreateReservation(Reservation reservation)
        {
            // 1. JWT'den kullanıcı ID'sini al
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            // 2. Odayı kontrol et
            var room = _context.Rooms
                .FirstOrDefault(x =>
                    x.Id == reservation.RoomId &&
                    !x.IsDeleted);

            if (room == null)
            {
                return NotFound("Oda bulunamadı.");
            }

            // 3. Tarihleri kontrol et
            if (reservation.CheckInDate >= reservation.CheckOutDate)
            {
                return BadRequest(
                    "Çıkış tarihi giriş tarihinden sonra olmalıdır.");
            }

            if (reservation.CheckInDate.Date < DateTime.Today)
            {
                return BadRequest(
                    "Geçmiş bir tarih için rezervasyon yapılamaz.");
            }

            // 4. Odanın seçilen tarihlerde başka rezervasyonu var mı?
            var hasConflict = _context.Reservations.Any(x =>
                x.RoomId == reservation.RoomId &&
                !x.IsDeleted &&
                x.Status == "Active" &&
                reservation.CheckInDate < x.CheckOutDate &&
                reservation.CheckOutDate > x.CheckInDate);

            if (hasConflict)
            {
                return BadRequest(
                    "Bu oda seçilen tarihlerde zaten rezerve edilmiş.");
            }

            // 5. Gece sayısını hesapla
            var numberOfNights =
                (reservation.CheckOutDate.Date -
                 reservation.CheckInDate.Date).Days;

            // 6. Toplam fiyatı sistem hesaplar
            reservation.TotalPrice =
                numberOfNights * room.PricePerNight;

            // 7. Kullanıcı ID'sini JWT'den al
            reservation.UserId = Guid.Parse(userId);

            // 8. Rezervasyon bilgilerini oluştur
            reservation.Id = Guid.NewGuid();
            reservation.Status = "Active";
            reservation.CancellationFee = 0;
            reservation.CreatedAt = DateTime.UtcNow;
            reservation.IsDeleted = false;

            // 9. Odayı müsait değil olarak işaretle
            room.IsAvailable = false;

            // 10. Veritabanına kaydet
            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            return Ok(reservation);
        }

        // PUT: api/Reservation/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateReservation(
            Guid id,
            Reservation updatedReservation)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            // 1. Mevcut rezervasyonu bul
            var reservation = _context.Reservations
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (reservation == null)
            {
                return NotFound("Rezervasyon bulunamadı.");
            }

            // 2. Ödeme yapılmış mı kontrol et
            var hasPayment = _context.Payments
                .Any(x =>
                    x.ReservationId == reservation.Id &&
                    x.PaymentType == "Payment" &&
                    x.Status == "Completed" &&
                    !x.IsDeleted);

            if (hasPayment)
            {
                return BadRequest(
                    "Ödeme yapılmış bir rezervasyon güncellenemez.");
            }

            // 3. Eski odayı kaydet
            var oldRoomId = reservation.RoomId;

            // 4. Yeni odayı kontrol et
            var newRoom = _context.Rooms
                .FirstOrDefault(x =>
                    x.Id == updatedReservation.RoomId &&
                    !x.IsDeleted);

            if (newRoom == null)
            {
                return NotFound("Oda bulunamadı.");
            }

            // 5. Tarih kontrolü
            if (updatedReservation.CheckInDate >=
                updatedReservation.CheckOutDate)
            {
                return BadRequest(
                    "Çıkış tarihi giriş tarihinden sonra olmalıdır.");
            }

            // 6. Geçmiş tarih kontrolü
            if (updatedReservation.CheckInDate.Date < DateTime.Today)
            {
                return BadRequest(
                    "Geçmiş bir tarih için rezervasyon yapılamaz.");
            }

            // 7. Yeni oda ve tarihlerde başka rezervasyon var mı?
            var hasConflict = _context.Reservations.Any(x =>
                x.Id != id &&
                x.RoomId == updatedReservation.RoomId &&
                !x.IsDeleted &&
                x.Status == "Active" &&
                updatedReservation.CheckInDate < x.CheckOutDate &&
                updatedReservation.CheckOutDate > x.CheckInDate);

            if (hasConflict)
            {
                return BadRequest(
                    "Bu oda seçilen tarihlerde zaten rezerve edilmiş.");
            }

            // 8. Gece sayısını hesapla
            var numberOfNights =
                (updatedReservation.CheckOutDate.Date -
                 updatedReservation.CheckInDate.Date).Days;

            // 9. Fiyatı sistem tekrar hesaplar
            reservation.TotalPrice =
                numberOfNights * newRoom.PricePerNight;

            // 10. Rezervasyon bilgilerini güncelle
            reservation.RoomId = updatedReservation.RoomId;
            reservation.CheckInDate = updatedReservation.CheckInDate;
            reservation.CheckOutDate = updatedReservation.CheckOutDate;

            // 11. Eğer oda değiştiyse eski odayı kontrol et
            if (oldRoomId != updatedReservation.RoomId)
            {
                var oldRoom = _context.Rooms
                    .FirstOrDefault(x =>
                        x.Id == oldRoomId &&
                        !x.IsDeleted);

                if (oldRoom != null)
                {
                    // Eski odada başka aktif rezervasyon var mı?
                    var oldRoomHasActiveReservation =
                        _context.Reservations.Any(x =>
                            x.Id != id &&
                            x.RoomId == oldRoomId &&
                            !x.IsDeleted &&
                            x.Status == "Active");

                    oldRoom.IsAvailable =
                        !oldRoomHasActiveReservation;
                }

                // Yeni odayı müsait değil olarak işaretle
                newRoom.IsAvailable = false;
            }
            else
            {
                // Oda değişmediyse mevcut oda müsait değil kalır
                newRoom.IsAvailable = false;
            }

            // 12. Kaydet
            _context.SaveChanges();

            return Ok(reservation);
        }

        // POST: api/Reservation/{id}/cancel
        [HttpPost("{id}/cancel")]
        public IActionResult CancelReservation(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            // 1. Rezervasyonu bul
            var reservation = _context.Reservations
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (reservation == null)
            {
                return NotFound("Rezervasyon bulunamadı.");
            }

            // 2. Rezervasyon zaten iptal edilmiş mi?
            if (reservation.Status == "Cancelled")
            {
                return BadRequest(
                    "Bu rezervasyon zaten iptal edilmiş.");
            }

            // 3. Giriş tarihine kalan gün sayısını hesapla
            var daysUntilCheckIn =
                (reservation.CheckInDate.Date - DateTime.Today).Days;

            // 4. İptal cezasını hesapla
            if (daysUntilCheckIn >= 2)
            {
                // 2 veya daha fazla gün kala
                reservation.CancellationFee = 0;
            }
            else if (daysUntilCheckIn == 1)
            {
                // 1 gün kala %30 ceza
                reservation.CancellationFee =
                    reservation.TotalPrice * 0.30m;
            }
            else
            {
                // Giriş günü %60 ceza
                reservation.CancellationFee =
                    reservation.TotalPrice * 0.60m;
            }

            // 5. Rezervasyonu iptal et
            reservation.Status = "Cancelled";

            // 6. Odayı tekrar müsait yap
            var room = _context.Rooms
                .FirstOrDefault(x =>
                    x.Id == reservation.RoomId &&
                    !x.IsDeleted);

            if (room != null)
            {
                room.IsAvailable = true;
            }

            // 7. Bu rezervasyon için yapılmış ödeme var mı?
            var payment = _context.Payments
                .FirstOrDefault(x =>
                    x.ReservationId == reservation.Id &&
                    x.PaymentType == "Payment" &&
                    x.Status == "Completed" &&
                    !x.IsDeleted);

            // 8. İade tutarını hesapla
            decimal refundAmount = 0;

            if (payment != null)
            {
                refundAmount =
                    payment.Amount - reservation.CancellationFee;

                // İade tutarı negatif olamaz
                if (refundAmount < 0)
                {
                    refundAmount = 0;
                }

                // 9. İade kaydı oluştur
                var refund = new Payment
                {
                    Id = Guid.NewGuid(),
                    ReservationId = reservation.Id,
                    Amount = refundAmount,
                    PaymentType = "Refund",
                    Status = "Completed",
                    TransactionDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Payments.Add(refund);
            }

            // 10. Değişiklikleri kaydet
            _context.SaveChanges();

            return Ok(new
            {
                message = "Rezervasyon başarıyla iptal edildi.",
                cancellationFee = reservation.CancellationFee,
                refundAmount = refundAmount
            });
        }
    }
}