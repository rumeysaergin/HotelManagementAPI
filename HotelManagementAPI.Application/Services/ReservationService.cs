using AutoMapper;
using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;

namespace HotelManagementAPI.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _repository;
        private readonly IRoomRepository _roomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IMapper _mapper;
        private readonly ErrorMessageService _errorMessageService;

        public ReservationService(
            IReservationRepository repository,
            IRoomRepository roomRepository,
            IUserRepository userRepository,
            IPaymentRepository paymentRepository,
            IMapper mapper,
            ErrorMessageService errorMessageService)
        {
            _repository = repository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
            _paymentRepository = paymentRepository;
            _mapper = mapper;
            _errorMessageService = errorMessageService;
        }

        public List<ReservationDto> GetReservations(
            Guid userId,
            Guid? hotelId)
        {
            var reservations =
                _repository.GetReservations(userId, hotelId);

            return _mapper.Map<List<ReservationDto>>(reservations);
        }

        public ReservationDto? CreateReservation(
            Guid userId,
            ReservationDto reservationDto)
        {
            var room =
                _roomRepository.GetById(reservationDto.RoomId);

            if (room == null)
            {
                return null;
            }

            if (reservationDto.CheckInDate >=
                reservationDto.CheckOutDate)
            {
                var error = _errorMessageService.Get(
                    "RESERVATION_INVALID_DATES");

                throw new ArgumentException(
                    $"{error.Code}: {error.Message}");
            }

            if (reservationDto.CheckInDate.Date <
                DateTime.Today)
            {
                var error = _errorMessageService.Get(
                    "RESERVATION_PAST_DATE");

                throw new ArgumentException(
                    $"{error.Code}: {error.Message}");
            }

            var hasConflict =
                _repository.HasConflict(
                    reservationDto.RoomId,
                    reservationDto.CheckInDate,
                    reservationDto.CheckOutDate);

            if (hasConflict)
            {
                var error = _errorMessageService.Get(
                    "RESERVATION_CONFLICT");

                throw new InvalidOperationException(
                    $"{error.Code}: {error.Message}");
            }

            var numberOfNights =
                (reservationDto.CheckOutDate.Date -
                 reservationDto.CheckInDate.Date).Days;

            var reservation =
                _mapper.Map<Reservation>(reservationDto);

            reservation.Id = Guid.NewGuid();
            reservation.UserId = userId;
            reservation.TotalPrice =
                numberOfNights * room.PricePerNight;
            reservation.Status = "Active";
            reservation.CancellationFee = 0;
            reservation.CreatedAt = DateTime.UtcNow;
            reservation.IsDeleted = false;

            room.IsAvailable = false;

            _repository.Add(reservation);
            _repository.SaveChanges();

            return _mapper.Map<ReservationDto>(reservation);
        }

        public ReservationDto? UpdateReservation(
            Guid userId,
            Guid id,
            ReservationDto reservationDto)
        {
            var reservation =
                _repository.GetById(id);

            if (reservation == null ||
                reservation.UserId != userId)
            {
                return null;
            }

            if (_repository.HasPayment(reservation.Id))
            {
                var error = _errorMessageService.Get(
                    "RESERVATION_PAID_UPDATE");

                throw new InvalidOperationException(
                    $"{error.Code}: {error.Message}");
            }

            var oldRoomId = reservation.RoomId;

            var newRoom =
                _roomRepository.GetById(
                    reservationDto.RoomId);

            if (newRoom == null)
            {
                return null;
            }

            if (reservationDto.CheckInDate >=
                reservationDto.CheckOutDate)
            {
                var error = _errorMessageService.Get(
                    "RESERVATION_INVALID_DATES");

                throw new ArgumentException(
                    $"{error.Code}: {error.Message}");
            }

            if (reservationDto.CheckInDate.Date <
                DateTime.Today)
            {
                var error = _errorMessageService.Get(
                    "RESERVATION_PAST_DATE");

                throw new ArgumentException(
                    $"{error.Code}: {error.Message}");
            }

            var hasConflict =
                _repository.HasConflict(
                    reservationDto.RoomId,
                    reservationDto.CheckInDate,
                    reservationDto.CheckOutDate,
                    id);

            if (hasConflict)
            {
                var error = _errorMessageService.Get(
                    "RESERVATION_CONFLICT");

                throw new InvalidOperationException(
                    $"{error.Code}: {error.Message}");
            }

            var numberOfNights =
                (reservationDto.CheckOutDate.Date -
                 reservationDto.CheckInDate.Date).Days;

            _mapper.Map(
                reservationDto,
                reservation);

            reservation.TotalPrice =
                numberOfNights * newRoom.PricePerNight;

            if (oldRoomId != reservationDto.RoomId)
            {
                var oldRoom =
                    _roomRepository.GetById(oldRoomId);

                if (oldRoom != null)
                {
                    var oldRoomHasActiveReservation =
                        _repository
                            .GetActiveReservationsByRoom(oldRoomId)
                            .Any(x => x.Id != id);

                    oldRoom.IsAvailable =
                        !oldRoomHasActiveReservation;
                }

                newRoom.IsAvailable = false;
            }
            else
            {
                newRoom.IsAvailable = false;
            }

            _repository.Update(reservation);
            _repository.SaveChanges();

            return _mapper.Map<ReservationDto>(reservation);
        }

        public object? CancelReservation(
            Guid userId,
            Guid id)
        {
            var reservation =
                _repository.GetById(id);

            if (reservation == null ||
                reservation.UserId != userId)
            {
                return null;
            }

            if (reservation.Status == "Cancelled")
            {
                var error = _errorMessageService.Get(
                    "RESERVATION_ALREADY_CANCELLED");

                throw new InvalidOperationException(
                    $"{error.Code}: {error.Message}");
            }

            var daysUntilCheckIn =
                (reservation.CheckInDate.Date -
                 DateTime.Today).Days;

            if (daysUntilCheckIn >= 2)
            {
                reservation.CancellationFee = 0;
            }
            else if (daysUntilCheckIn == 1)
            {
                reservation.CancellationFee =
                    reservation.TotalPrice * 0.30m;
            }
            else
            {
                reservation.CancellationFee =
                    reservation.TotalPrice * 0.60m;
            }

            reservation.Status = "Cancelled";

            var room =
                _roomRepository.GetById(
                    reservation.RoomId);

            if (room != null)
            {
                room.IsAvailable = true;
            }

            var payment =
                _paymentRepository.GetPaymentByReservationId(
                    reservation.Id);

            decimal refundAmount = 0;

            if (payment != null)
            {
                refundAmount =
                    reservation.TotalPrice -
                    reservation.CancellationFee;

                if (refundAmount > 0)
                {
                    _userRepository.UpdateBalance(
                        userId,
                        refundAmount);

                    var refundPayment = new Payment
                    {
                        Id = Guid.NewGuid(),
                        ReservationId = reservation.Id,
                        Amount = refundAmount,
                        PaymentType = "Refund",
                        Status = "Completed",
                        TransactionDate = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    _paymentRepository.Add(refundPayment);
                }
            }

            _repository.Update(reservation);

            _repository.SaveChanges();
            _userRepository.SaveChanges();
            _paymentRepository.SaveChanges();

            return new
            {
                message =
                    "Rezervasyon başarıyla iptal edildi.",
                cancellationFee =
                    reservation.CancellationFee,
                refundAmount =
                    refundAmount
            };
        }
    }
}