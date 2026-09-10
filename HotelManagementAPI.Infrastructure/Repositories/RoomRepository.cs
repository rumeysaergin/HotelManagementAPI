using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;
using HotelManagementAPI.Infrastructure.Data;

namespace HotelManagementAPI.Infrastructure.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly HotelManagementDbContext _context;

        public RoomRepository(HotelManagementDbContext context)
        {
            _context = context;
        }

        public List<Room> GetRooms(Guid? hotelId)
        {
            var query = _context.Rooms
                .Where(x => !x.IsDeleted);

            if (hotelId.HasValue)
            {
                query = query
                    .Where(x => x.HotelId == hotelId.Value);
            }

            return query
                .OrderBy(x => x.Id)
                .ToList();
        }

        public Room Add(Room room)
        {
            _context.Rooms.Add(room);
            return room;
        }

        public Room? GetById(Guid id)
        {
            return _context.Rooms
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public void Update(Room room)
        {
            _context.Rooms.Update(room);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}