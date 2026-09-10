using HotelManagementAPI.Domain.Entities;

namespace HotelManagementAPI.Application.Repositories
{
    public interface IPaymentRepository
    {
        Payment Add(Payment payment);

        Payment? GetPaymentByReservationId(
            Guid reservationId);

        List<Payment> GetPayments(Guid userId, Guid? hotelId);

        void SaveChanges();
    }
}