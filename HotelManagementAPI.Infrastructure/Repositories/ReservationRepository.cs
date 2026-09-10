using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;
using HotelManagementAPI.Infrastructure.Data;

namespace HotelManagementAPI.Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly HotelManagementDbContext _context;

        public ReservationRepository(HotelManagementDbContext context)
        {
            _context = context;
        }

        public List<Reservation> GetReservations(Guid userId, Guid? hotelId)
        {
            var query = _context.Reservations
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsDeleted);

            if (hotelId.HasValue)
            {
                query = query.Where(x =>
                    _context.Rooms.Any(r =>
                        r.Id == x.RoomId &&
                        r.HotelId == hotelId.Value &&
                        !r.IsDeleted));
            }

            return query
                .OrderBy(x => x.Id)
                .ToList();
        }

        public Reservation? GetById(Guid id)
        {
            return _context.Reservations
                .FirstOrDefault(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        public Reservation Add(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            return reservation;
        }

        public void Update(Reservation reservation)
        {
            _context.Reservations.Update(reservation);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }

        public bool HasConflict(
            Guid roomId,
            DateTime checkInDate,
            DateTime checkOutDate,
            Guid? excludeReservationId = null)
        {
            return _context.Reservations.Any(x =>
                (!excludeReservationId.HasValue ||
                 x.Id != excludeReservationId.Value) &&
                x.RoomId == roomId &&
                !x.IsDeleted &&
                x.Status == "Active" &&
                checkInDate < x.CheckOutDate &&
                checkOutDate > x.CheckInDate);
        }

        public bool HasPayment(Guid reservationId)
        {
            return _context.Payments.Any(x =>
                x.ReservationId == reservationId &&
                x.PaymentType == "Payment" &&
                x.Status == "Completed" &&
                !x.IsDeleted);
        }

        public List<Reservation> GetActiveReservationsByRoom(Guid roomId)
        {
            return _context.Reservations
                .Where(x =>
                    x.RoomId == roomId &&
                    !x.IsDeleted &&
                    x.Status == "Active")
                .ToList();
        }
    }
}