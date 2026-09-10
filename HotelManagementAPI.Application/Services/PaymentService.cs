using AutoMapper;
using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;

namespace HotelManagementAPI.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IRoomRepository _roomRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ErrorMessageService _errorMessageService;

        public PaymentService(
            IPaymentRepository paymentRepository,
            IReservationRepository reservationRepository,
            IRoomRepository roomRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ErrorMessageService errorMessageService)
        {
            _paymentRepository = paymentRepository;
            _reservationRepository = reservationRepository;
            _roomRepository = roomRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _errorMessageService = errorMessageService;
        }

        public object MakePayment(
            Guid userId,
            Guid reservationId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
                return null;

            var reservation =
                _reservationRepository.GetById(reservationId);

            if (reservation == null ||
                reservation.UserId != userId)
                return null;

            if (reservation.Status == "Cancelled")
            {
                var error = _errorMessageService.Get(
                    "PAYMENT_CANCELLED_RESERVATION");

                throw new InvalidOperationException(
                    $"{error.Code}: {error.Message}");
            }

            var existingPayment =
                _paymentRepository.GetPaymentByReservationId(
                    reservationId);

            if (existingPayment != null)
            {
                var error = _errorMessageService.Get(
                    "PAYMENT_ALREADY_PAID");

                throw new InvalidOperationException(
                    $"{error.Code}: {error.Message}");
            }

            if (user.Balance < reservation.TotalPrice)
            {
                var error = _errorMessageService.Get(
                    "PAYMENT_INSUFFICIENT_BALANCE");

                throw new InvalidOperationException(
                    $"{error.Code}: {error.Message}");
            }

            _userRepository.UpdateBalance(
                userId,
                -reservation.TotalPrice);

            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                ReservationId = reservation.Id,
                Amount = reservation.TotalPrice,
                PaymentType = "Payment",
                Status = "Completed",
                TransactionDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _paymentRepository.Add(payment);
            _paymentRepository.SaveChanges();
            _userRepository.SaveChanges();

            var paymentDto =
                _mapper.Map<PaymentDto>(payment);

            return new
            {
                message = "Ödeme başarıyla gerçekleştirildi.",
                payment = paymentDto,
                remainingBalance = user.Balance
            };
        }

        public List<PaymentDto> GetPayments(
            Guid userId,
            Guid? hotelId)
        {
            var payments =
                _paymentRepository.GetPayments(
                    userId,
                    hotelId);

            return _mapper.Map<List<PaymentDto>>(payments);
        }
    }
}