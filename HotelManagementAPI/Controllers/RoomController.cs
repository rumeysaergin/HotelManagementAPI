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
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // GET: api/Room?hotelId=...
        [HttpGet]
        public IActionResult GetRooms(Guid? hotelId)
        {
            var rooms = _roomService.GetRooms(hotelId);

            return Ok(rooms);
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

            var result = _roomService.CreateRoom(
                Guid.Parse(userId),
                roomDto);

            if (result == null)
            {
                return NotFound(
                    "Belirtilen otel bulunamadı veya bu otel size ait değil.");
            }

            return Ok(result);
        }

        // PUT: api/Room/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateRoom(
            Guid id,
            RoomDto updatedRoomDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var result = _roomService.UpdateRoom(
                    Guid.Parse(userId),
                    id,
                    updatedRoomDto);

                if (result == null)
                {
                    return NotFound("Oda bulunamadı.");
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
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

            try
            {
                var result = _roomService.DeleteRoom(
                    Guid.Parse(userId),
                    id);

                if (!result)
                {
                    return NotFound("Oda bulunamadı.");
                }

                return Ok("Oda başarıyla silindi.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}