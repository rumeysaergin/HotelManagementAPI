using HotelManagementAPI.Application.DTOs;

namespace HotelManagementAPI.Application.Services
{
    public interface IReservationService
    {
        List<ReservationDto> GetReservations(Guid userId, Guid? hotelId);

        ReservationDto? CreateReservation(
            Guid userId,
            ReservationDto reservationDto);

        ReservationDto? UpdateReservation(
            Guid userId,
            Guid id,
            ReservationDto reservationDto);

        object? CancelReservation(Guid userId, Guid id);
    }
}