using HotelManagementAPI.Data;
using HotelManagementAPI.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public IActionResult GetHotels()
        {
            var hotels = _context.Hotels
                .Where(x => !x.IsDeleted)
                .ToList();

            return Ok(hotels);
        }
    }
}