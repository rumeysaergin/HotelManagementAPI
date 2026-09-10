using HotelManagementAPI.Application.DTOs;

namespace HotelManagementAPI.Application.Services
{
    public interface IHotelService
    {
        List<HotelDto> GetHotels(Guid userId);

        HotelDto CreateHotel(Guid userId, HotelDto hotelDto);

        HotelDto? UpdateHotel(
            Guid userId,
            Guid id,
            HotelDto hotelDto);

        bool DeleteHotel(Guid userId, Guid id);
    }
}