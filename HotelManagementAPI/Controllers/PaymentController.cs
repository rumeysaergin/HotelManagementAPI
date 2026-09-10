using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Services;
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
        private readonly IPaymentService _paymentService;

        public PaymentController(
            IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("{reservationId}")]
        public IActionResult MakePayment(Guid reservationId)
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result =
                    _paymentService.MakePayment(
                        Guid.Parse(userId),
                        reservationId);

                if (result == null)
                {
                    return NotFound(
                        "Rezervasyon bulunamadı.");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetPayments(Guid? hotelId)
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var payments =
                _paymentService.GetPayments(
                    Guid.Parse(userId),
                    hotelId);

            return Ok(payments);
        }
    }
}