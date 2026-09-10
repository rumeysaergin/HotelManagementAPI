using HotelManagementAPI.Application.DTOs;

namespace HotelManagementAPI.Application.Services
{
    public interface IUserService
    {
        object Register(UserDto userDto);

        string? Login(string email, string password);

        UserDto? GetProfile(Guid userId);

        UserDto? UpdateUser(
            Guid userId,
            UserDto updatedUserDto);
    }
}