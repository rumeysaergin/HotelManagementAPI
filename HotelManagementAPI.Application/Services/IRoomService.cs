using HotelManagementAPI.Application.DTOs;

namespace HotelManagementAPI.Application.Services
{
    public interface IRoomService
    {
        List<RoomDto> GetRooms(Guid? hotelId);

        RoomDto? CreateRoom(Guid userId, RoomDto roomDto);

        RoomDto? UpdateRoom(
            Guid userId,
            Guid id,
            RoomDto roomDto);

        bool DeleteRoom(Guid userId, Guid id);
    }
}