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

        [HttpPost]
        public IActionResult CreateReservation(Reservation reservation)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var room = _context.Rooms
                .FirstOrDefault(x =>
                    x.Id == reservation.RoomId &&
                    !x.IsDeleted);

            if (room == null)
            {
                return NotFound("Oda bulunamadı.");
            }

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

            var numberOfNights =
                (reservation.CheckOutDate.Date -
                 reservation.CheckInDate.Date).Days;

            reservation.TotalPrice =
                numberOfNights * room.PricePerNight;

            reservation.UserId = Guid.Parse(userId);
            reservation.Id = Guid.NewGuid();
            reservation.Status = "Active";
            reservation.CancellationFee = 0;
            reservation.CreatedAt = DateTime.UtcNow;
            reservation.IsDeleted = false;

            room.IsAvailable = false;

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            return Ok(reservation);
        }

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

            var reservation = _context.Reservations
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (reservation == null)
            {
                return NotFound("Rezervasyon bulunamadı.");
            }

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

            var oldRoomId = reservation.RoomId;

            var newRoom = _context.Rooms
                .FirstOrDefault(x =>
                    x.Id == updatedReservation.RoomId &&
                    !x.IsDeleted);

            if (newRoom == null)
            {
                return NotFound("Oda bulunamadı.");
            }

            if (updatedReservation.CheckInDate >=
                updatedReservation.CheckOutDate)
            {
                return BadRequest(
                    "Çıkış tarihi giriş tarihinden sonra olmalıdır.");
            }

            if (updatedReservation.CheckInDate.Date < DateTime.Today)
            {
                return BadRequest(
                    "Geçmiş bir tarih için rezervasyon yapılamaz.");
            }

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

            var numberOfNights =
                (updatedReservation.CheckOutDate.Date -
                 updatedReservation.CheckInDate.Date).Days;

            reservation.TotalPrice =
                numberOfNights * newRoom.PricePerNight;

            reservation.RoomId = updatedReservation.RoomId;
            reservation.CheckInDate = updatedReservation.CheckInDate;
            reservation.CheckOutDate = updatedReservation.CheckOutDate;

            if (oldRoomId != updatedReservation.RoomId)
            {
                var oldRoom = _context.Rooms
                    .FirstOrDefault(x =>
                        x.Id == oldRoomId &&
                        !x.IsDeleted);

                if (oldRoom != null)
                {
                    var oldRoomHasActiveReservation =
                        _context.Reservations.Any(x =>
                            x.Id != id &&
                            x.RoomId == oldRoomId &&
                            !x.IsDeleted &&
                            x.Status == "Active");

                    oldRoom.IsAvailable =
                        !oldRoomHasActiveReservation;
                }

                newRoom.IsAvailable = false;
            }
            else
            {
                newRoom.IsAvailable = false;
            }

            _context.SaveChanges();

            return Ok(reservation);
        }

        [HttpPost("{id}/cancel")]
        public IActionResult CancelReservation(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var reservation = _context.Reservations
                .FirstOrDefault(x =>
                    x.Id == id &&
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (reservation == null)
            {
                return NotFound("Rezervasyon bulunamadı.");
            }

            if (reservation.Status == "Cancelled")
            {
                return BadRequest(
                    "Bu rezervasyon zaten iptal edilmiş.");
            }

            var daysUntilCheckIn =
                (reservation.CheckInDate.Date - DateTime.Today).Days;

            if (daysUntilCheckIn >= 2)
            {
                reservation.CancellationFee = 0;
            }
            else if (daysUntilCheckIn == 1)
            {
                reservation.CancellationFee =
                    reservation.TotalPrice * 0.30m;
            }
            else
            {
                reservation.CancellationFee =
                    reservation.TotalPrice * 0.60m;
            }

            reservation.Status = "Cancelled";

            var room = _context.Rooms
                .FirstOrDefault(x =>
                    x.Id == reservation.RoomId &&
                    !x.IsDeleted);

            if (room != null)
            {
                room.IsAvailable = true;
            }

            var payment = _context.Payments
                .FirstOrDefault(x =>
                    x.ReservationId == reservation.Id &&
                    x.PaymentType == "Payment" &&
                    x.Status == "Completed" &&
                    !x.IsDeleted);

            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Id == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            decimal refundAmount = 0;

            if (payment != null)
            {
                refundAmount =
                    payment.Amount - reservation.CancellationFee;

                if (refundAmount < 0)
                {
                    refundAmount = 0;
                }

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

                user.Balance += refundAmount;
            }

            _context.SaveChanges();

            return Ok(new
            {
                message = "Rezervasyon başarıyla iptal edildi.",
                cancellationFee = reservation.CancellationFee,
                refundAmount = refundAmount,
                remainingBalance = user.Balance
            });
        }
    }
}