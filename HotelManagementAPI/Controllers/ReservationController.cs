using AutoMapper;
using HotelManagementAPI.Data;
using HotelManagementAPI.Entities;
using HotelManagementAPI.Models.DTOs;
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
        private readonly IMapper _mapper;

        public ReservationController(
            HotelManagementDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

            var reservationDtos =
                _mapper.Map<List<ReservationDto>>(reservations);

            return Ok(reservationDtos);
        }

        [HttpPost]
        public IActionResult CreateReservation(
            ReservationDto reservationDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var room = _context.Rooms
                .FirstOrDefault(x =>
                    x.Id == reservationDto.RoomId &&
                    !x.IsDeleted);

            if (room == null)
            {
                return NotFound("Oda bulunamadı.");
            }

            if (reservationDto.CheckInDate >=
                reservationDto.CheckOutDate)
            {
                return BadRequest(
                    "Çıkış tarihi giriş tarihinden sonra olmalıdır.");
            }

            if (reservationDto.CheckInDate.Date < DateTime.Today)
            {
                return BadRequest(
                    "Geçmiş bir tarih için rezervasyon yapılamaz.");
            }

            var hasConflict = _context.Reservations.Any(x =>
                x.RoomId == reservationDto.RoomId &&
                !x.IsDeleted &&
                x.Status == "Active" &&
                reservationDto.CheckInDate < x.CheckOutDate &&
                reservationDto.CheckOutDate > x.CheckInDate);

            if (hasConflict)
            {
                return BadRequest(
                    "Bu oda seçilen tarihlerde zaten rezerve edilmiş.");
            }

            var numberOfNights =
                (reservationDto.CheckOutDate.Date -
                 reservationDto.CheckInDate.Date).Days;

            var reservation = _mapper.Map<Reservation>(reservationDto);

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

            var result =
                _mapper.Map<ReservationDto>(reservation);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateReservation(
            Guid id,
            ReservationDto updatedReservationDto)
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
                    x.Id == updatedReservationDto.RoomId &&
                    !x.IsDeleted);

            if (newRoom == null)
            {
                return NotFound("Oda bulunamadı.");
            }

            if (updatedReservationDto.CheckInDate >=
                updatedReservationDto.CheckOutDate)
            {
                return BadRequest(
                    "Çıkış tarihi giriş tarihinden sonra olmalıdır.");
            }

            if (updatedReservationDto.CheckInDate.Date <
                DateTime.Today)
            {
                return BadRequest(
                    "Geçmiş bir tarih için rezervasyon yapılamaz.");
            }

            var hasConflict = _context.Reservations.Any(x =>
                x.Id != id &&
                x.RoomId == updatedReservationDto.RoomId &&
                !x.IsDeleted &&
                x.Status == "Active" &&
                updatedReservationDto.CheckInDate < x.CheckOutDate &&
                updatedReservationDto.CheckOutDate > x.CheckInDate);

            if (hasConflict)
            {
                return BadRequest(
                    "Bu oda seçilen tarihlerde zaten rezerve edilmiş.");
            }

            var numberOfNights =
                (updatedReservationDto.CheckOutDate.Date -
                 updatedReservationDto.CheckInDate.Date).Days;

            reservation.TotalPrice =
                numberOfNights * newRoom.PricePerNight;

            _mapper.Map(updatedReservationDto, reservation);

            if (oldRoomId != updatedReservationDto.RoomId)
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

            var result =
                _mapper.Map<ReservationDto>(reservation);

            return Ok(result);
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
                (reservation.CheckInDate.Date -
                 DateTime.Today).Days;

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