using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;
using HotelManagementAPI.Infrastructure.Data;

namespace HotelManagementAPI.Infrastructure.Repositories
{
    public class HotelRepository : IHotelRepository
    {
        private readonly HotelManagementDbContext _context;

        public HotelRepository(HotelManagementDbContext context)
        {
            _context = context;
        }

        public List<Hotel> GetHotels(Guid userId)
        {
            return _context.Hotels
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToList();
        }

        public Hotel Add(Hotel hotel)
        {
            _context.Hotels.Add(hotel);
            return hotel;
        }

        public Hotel? GetById(Guid id)
        {
            return _context.Hotels
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public void Update(Hotel hotel)
        {
            _context.Hotels.Update(hotel);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}