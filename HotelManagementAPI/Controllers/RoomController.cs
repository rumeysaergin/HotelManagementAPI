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
    public class RoomController : ControllerBase
    {
        private readonly HotelManagementDbContext _context;

        public RoomController(HotelManagementDbContext context)
        {
            _context = context;
        }

        // GET: api/Room
        [HttpGet]
        public IActionResult GetRooms()
        {
            var rooms = _context.Rooms
                .Where(x => !x.IsDeleted)
                .ToList();

            return Ok(rooms);
        }

        // POST: api/Room
        [HttpPost]
        public IActionResult CreateRoom(Room room)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var hotel = _context.Hotels
                .FirstOrDefault(x =>
                    x.Id == room.HotelId &&
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (hotel == null)
            {
                return NotFound(
                    "Belirtilen otel bulunamadı veya bu otel size ait değil.");
            }

            room.Id = Guid.NewGuid();
            room.IsDeleted = false;
            room.IsAvailable = true;

            _context.Rooms.Add(room);
            _context.SaveChanges();

            return Ok(room);
        }

        // PUT: api/Room/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateRoom(Guid id, Room updatedRoom)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var room = _context.Rooms
                .FirstOrDefault(x => x.Id == id && !x.IsDeleted);

            if (room == null)
            {
                return NotFound("Oda bulunamadı.");
            }

            var hotel = _context.Hotels
                .FirstOrDefault(x =>
                    x.Id == room.HotelId &&
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (hotel == null)
            {
                return Unauthorized(
                    "Bu odayı güncelleme yetkiniz yok.");
            }

            room.RoomNumber = updatedRoom.RoomNumber;
            room.RoomType = updatedRoom.RoomType;
            room.Capacity = updatedRoom.Capacity;
            room.PricePerNight = updatedRoom.PricePerNight;

            _context.SaveChanges();

            return Ok(room);
        }
        // DELETE: api/Room/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteRoom(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var room = _context.Rooms
                .FirstOrDefault(x => x.Id == id && !x.IsDeleted);

            if (room == null)
            {
                return NotFound("Oda bulunamadı.");
            }

            var hotel = _context.Hotels
                .FirstOrDefault(x =>
                    x.Id == room.HotelId &&
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (hotel == null)
            {
                return Unauthorized(
                    "Bu odayı silme yetkiniz yok.");
            }

            room.IsDeleted = true;

            _context.SaveChanges();

            return Ok("Oda başarıyla silindi.");
        }
    }
}