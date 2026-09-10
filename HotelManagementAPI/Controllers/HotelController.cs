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
    public class HotelController : ControllerBase
    {
        private readonly IHotelService _hotelService;

        public HotelController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        // GET: api/Hotel
        [HttpGet]
        public IActionResult GetHotels()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var hotels = _hotelService.GetHotels(Guid.Parse(userId));

            return Ok(hotels);
        }

        // POST: api/Hotel
        [HttpPost]
        public IActionResult CreateHotel(HotelDto hotelDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = _hotelService.CreateHotel(
                Guid.Parse(userId),
                hotelDto);

            return Ok(result);
        }

        // PUT: api/Hotel/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateHotel(
            Guid id,
            HotelDto updatedHotelDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = _hotelService.UpdateHotel(
                    Guid.Parse(userId),
                    id,
                    updatedHotelDto);

                if (result == null)
                {
                    return NotFound("Otel bulunamadı.");
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        // DELETE: api/Hotel/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteHotel(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = _hotelService.DeleteHotel(
                    Guid.Parse(userId),
                    id);

                if (!result)
                {
                    return NotFound("Otel bulunamadı.");
                }

                return Ok("Otel başarıyla silindi.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}