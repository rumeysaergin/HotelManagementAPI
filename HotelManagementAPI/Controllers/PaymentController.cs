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
    public class PaymentController : ControllerBase
    {
        private readonly HotelManagementDbContext _context;
        private readonly IMapper _mapper;

        public PaymentController(
            HotelManagementDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("{reservationId}")]
        public IActionResult MakePayment(Guid reservationId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Id == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı.");
            }

            var reservation = _context.Reservations
                .FirstOrDefault(x =>
                    x.Id == reservationId &&
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (reservation == null)
            {
                return NotFound("Rezervasyon bulunamadı.");
            }

            if (reservation.Status == "Cancelled")
            {
                return BadRequest(
                    "İptal edilmiş rezervasyon için ödeme yapılamaz.");
            }

            var existingPayment = _context.Payments
                .FirstOrDefault(x =>
                    x.ReservationId == reservationId &&
                    x.PaymentType == "Payment" &&
                    x.Status == "Completed" &&
                    !x.IsDeleted);

            if (existingPayment != null)
            {
                return BadRequest(
                    "Bu rezervasyonun ödemesi zaten yapılmış.");
            }

            if (user.Balance < reservation.TotalPrice)
            {
                return BadRequest(
                    "Yetersiz bakiye.");
            }

            user.Balance -= reservation.TotalPrice;

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                ReservationId = reservation.Id,
                Amount = reservation.TotalPrice,
                PaymentType = "Payment",
                Status = "Completed",
                TransactionDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Payments.Add(payment);
            _context.SaveChanges();

            var paymentDto = _mapper.Map<PaymentDto>(payment);

            return Ok(new
            {
                message = "Ödeme başarıyla gerçekleştirildi.",
                payment = paymentDto,
                remainingBalance = user.Balance
            });
        }

        [HttpGet]
        public IActionResult GetPayments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var payments = _context.Payments
                .Where(x =>
                    !x.IsDeleted &&
                    _context.Reservations.Any(r =>
                        r.Id == x.ReservationId &&
                        r.UserId == Guid.Parse(userId) &&
                        !r.IsDeleted))
                .ToList();

            var paymentDtos =
                _mapper.Map<List<PaymentDto>>(payments);

            return Ok(paymentDtos);
        }
    }
}