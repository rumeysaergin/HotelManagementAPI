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
    public class HotelController : ControllerBase
    {
        private readonly HotelManagementDbContext _context;

        public HotelController(HotelManagementDbContext context)
        {
            _context = context;
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

            var hotels = _context.Hotels
                .Where(x =>
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted)
                .ToList();

            return Ok(hotels);
        }

        // POST: api/Hotel
        [HttpPost]
        public IActionResult CreateHotel(Hotel hotel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            hotel.Id = Guid.NewGuid();
            hotel.UserId = Guid.Parse(userId);
            hotel.IsDeleted = false;

            _context.Hotels.Add(hotel);
            _context.SaveChanges();

            return Ok(hotel);
        }

        // PUT: api/Hotel/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateHotel(Guid id, Hotel updatedHotel)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var hotel = _context.Hotels
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (hotel == null)
            {
                return NotFound("Otel bulunamadı.");
            }

            // Otelin mevcut kullanıcıya ait olup olmadığını kontrol et
            if (hotel.UserId != Guid.Parse(userId))
            {
                return Unauthorized(
                    "Bu oteli güncelleme yetkiniz yok.");
            }

            hotel.Name = updatedHotel.Name;
            hotel.Address = updatedHotel.Address;
            hotel.City = updatedHotel.City;

            _context.SaveChanges();

            return Ok(hotel);
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

            var hotel = _context.Hotels
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted);

            if (hotel == null)
            {
                return NotFound("Otel bulunamadı.");
            }

            // Otelin mevcut kullanıcıya ait olup olmadığını kontrol et
            if (hotel.UserId != Guid.Parse(userId))
            {
                return Unauthorized(
                    "Bu oteli silme yetkiniz yok.");
            }

            hotel.IsDeleted = true;

            _context.SaveChanges();

            return Ok("Otel başarıyla silindi.");
        }
    }
}