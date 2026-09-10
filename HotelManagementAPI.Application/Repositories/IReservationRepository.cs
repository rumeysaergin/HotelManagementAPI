using HotelManagementAPI.Domain.Entities;

namespace HotelManagementAPI.Application.Repositories
{
    public interface IReservationRepository
    {
        List<Reservation> GetReservations(Guid userId, Guid? hotelId);
        Reservation? GetById(Guid id);
        Reservation Add(Reservation reservation);
        void Update(Reservation reservation);
        void SaveChanges();

        bool HasConflict(
            Guid roomId,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? excludeReservationId = null);

        bool HasPayment(Guid reservationId);

        List<Reservation> GetActiveReservationsByRoom(Guid roomId);
    }
}