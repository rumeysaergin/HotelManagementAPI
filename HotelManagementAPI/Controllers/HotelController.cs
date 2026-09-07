using HotelManagementAPI.Data;
using HotelManagementAPI.Entities;
using HotelManagementAPI.Models.DTOs;
using AutoMapper;
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
        private readonly IMapper _mapper;

        public HotelController(
            HotelManagementDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

            var hotelDtos = _mapper.Map<List<HotelDto>>(hotels);

            return Ok(hotelDtos);
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

            var hotel = _mapper.Map<Hotel>(hotelDto);

            hotel.Id = Guid.NewGuid();
            hotel.UserId = Guid.Parse(userId);
            hotel.IsDeleted = false;

            _context.Hotels.Add(hotel);
            _context.SaveChanges();

            var result = _mapper.Map<HotelDto>(hotel);

            return Ok(result);
        }

        // PUT: api/Hotel/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateHotel(Guid id, HotelDto updatedHotelDto)
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

            _mapper.Map(updatedHotelDto, hotel);

            _context.SaveChanges();

            var result = _mapper.Map<HotelDto>(hotel);

            return Ok(result);
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