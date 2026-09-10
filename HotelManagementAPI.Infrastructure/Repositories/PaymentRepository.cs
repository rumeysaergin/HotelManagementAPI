using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;
using HotelManagementAPI.Infrastructure.Data;

namespace HotelManagementAPI.Infrastructure.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly HotelManagementDbContext _context;

        public PaymentRepository(HotelManagementDbContext context)
        {
            _context = context;
        }

        public Payment Add(Payment payment)
        {
            _context.Payments.Add(payment);
            return payment;
        }

        public Payment? GetPaymentByReservationId(
            Guid reservationId)
        {
            return _context.Payments
                .FirstOrDefault(x =>
                    x.ReservationId == reservationId &&
                    x.PaymentType == "Payment" &&
                    x.Status == "Completed" &&
                    !x.IsDeleted);
        }

        public List<Payment> GetPayments(
            Guid userId,
            Guid? hotelId)
        {
            var query = _context.Payments
                .Where(x =>
                    !x.IsDeleted &&
                    _context.Reservations.Any(r =>
                        r.Id == x.ReservationId &&
                        r.UserId == userId &&
                        !r.IsDeleted));

            if (hotelId.HasValue)
            {
                query = query.Where(x =>
                    _context.Reservations.Any(r =>
                        r.Id == x.ReservationId &&
                        !r.IsDeleted &&
                        _context.Rooms.Any(room =>
                            room.Id == r.RoomId &&
                            room.HotelId == hotelId.Value &&
                            !room.IsDeleted)));
            }

            return query
                .OrderBy(x => x.Id)
                .ToList();
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}