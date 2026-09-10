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
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(
            IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        public IActionResult GetReservations(Guid? hotelId)
        {
            var userId =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var reservations =
                _reservationService.GetReservations(
                    Guid.Parse(userId),
                    hotelId);

            return Ok(reservations);
        }

        [HttpPost]
        public IActionResult CreateReservation(
            ReservationDto reservationDto)
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
                    _reservationService.CreateReservation(
                        Guid.Parse(userId),
                        reservationDto);

                if (result == null)
                {
                    return NotFound("Oda bulunamadı.");
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public IActionResult UpdateReservation(
            Guid id,
            ReservationDto updatedReservationDto)
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
                    _reservationService.UpdateReservation(
                        Guid.Parse(userId),
                        id,
                        updatedReservationDto);

                if (result == null)
                {
                    return NotFound(
                        "Rezervasyon veya oda bulunamadı.");
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        public IActionResult CancelReservation(Guid id)
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
                    _reservationService.CancelReservation(
                        Guid.Parse(userId),
                        id);

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
    }
}