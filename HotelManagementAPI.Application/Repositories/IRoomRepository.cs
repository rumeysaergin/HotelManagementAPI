using HotelManagementAPI.Domain.Entities;

namespace HotelManagementAPI.Application.Repositories
{
    public interface IRoomRepository
    {
        List<Room> GetRooms(Guid? hotelId);

        Room Add(Room room);

        Room? GetById(Guid id);

        void Update(Room room);

        void SaveChanges();
    }
}