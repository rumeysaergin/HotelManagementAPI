using HotelManagementAPI.Domain.Entities;

namespace HotelManagementAPI.Application.Repositories
{
    public interface IHotelRepository
    {
        List<Hotel> GetHotels(Guid userId);

        Hotel Add(Hotel hotel);

        Hotel? GetById(Guid id);

        void Update(Hotel hotel);

        void SaveChanges();
    }
}