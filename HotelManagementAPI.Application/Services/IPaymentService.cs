using HotelManagementAPI.Application.DTOs;

namespace HotelManagementAPI.Application.Services
{
    public interface IPaymentService
    {
        object? MakePayment(
            Guid userId,
            Guid reservationId);

        List<PaymentDto> GetPayments(
            Guid userId,
            Guid? hotelId);
    }
}