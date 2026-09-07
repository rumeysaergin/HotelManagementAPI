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
    public class RoomController : ControllerBase
    {
        private readonly HotelManagementDbContext _context;
        private readonly IMapper _mapper;

        public RoomController(
            HotelManagementDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Room
        [HttpGet]
        public IActionResult GetRooms()
        {
            var rooms = _context.Rooms
                .Where(x => !x.IsDeleted)
                .ToList();

            var roomDtos = _mapper.Map<List<RoomDto>>(rooms);

            return Ok(roomDtos);
        }

        // POST: api/Room
        [HttpPost]
        public IActionResult CreateRoom(RoomDto roomDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var hotel = _context.Hotels
                .FirstOrDefault(x =>
                    x.Id == roomDto.HotelId &&
                    x.UserId == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (hotel == null)
            {
                return NotFound(
                    "Belirtilen otel bulunamadı veya bu otel size ait değil.");
            }

            var room = _mapper.Map<Room>(roomDto);

            room.Id = Guid.NewGuid();
            room.IsDeleted = false;
            room.IsAvailable = true;

            _context.Rooms.Add(room);
            _context.SaveChanges();

            var result = _mapper.Map<RoomDto>(room);

            return Ok(result);
        }

        // PUT: api/Room/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateRoom(Guid id, RoomDto updatedRoomDto)
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

            _mapper.Map(updatedRoomDto, room);

            _context.SaveChanges();

            var result = _mapper.Map<RoomDto>(room);

            return Ok(result);
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