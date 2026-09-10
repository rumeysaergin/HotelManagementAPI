using AutoMapper;
using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Application.Services;
using HotelManagementAPI.Domain.Entities;
using Moq;

namespace HotelManagementAPI.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IReservationRepository> _reservationRepositoryMock;
        private readonly Mock<IRoomRepository> _roomRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly ErrorMessageService _errorMessageService;

        private readonly PaymentService _paymentService;

        public PaymentServiceTests()
        {
            _paymentRepositoryMock =
                new Mock<IPaymentRepository>();

            _reservationRepositoryMock =
                new Mock<IReservationRepository>();

            _roomRepositoryMock =
                new Mock<IRoomRepository>();

            _userRepositoryMock =
                new Mock<IUserRepository>();

            _mapperMock =
                new Mock<IMapper>();

            var trPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "errorMessages.tr.json");

            var enPath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "errorMessages.en.json");

            _errorMessageService =
                new ErrorMessageService(
                    trPath,
                    enPath,
                    new Microsoft.AspNetCore.Http.HttpContextAccessor());

            _paymentService =
                new PaymentService(
                    _paymentRepositoryMock.Object,
                    _reservationRepositoryMock.Object,
                    _roomRepositoryMock.Object,
                    _userRepositoryMock.Object,
                    _mapperMock.Object,
                    _errorMessageService);
        }

        [Fact]
        public void MakePayment_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            _userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns((User?)null);

            // Act
            var result =
                _paymentService.MakePayment(
                    userId,
                    reservationId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MakePayment_ShouldReturnNull_WhenReservationDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Balance = 10000
            };

            _userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns(user);

            _reservationRepositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns((Reservation?)null);

            // Act
            var result =
                _paymentService.MakePayment(
                    userId,
                    reservationId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MakePayment_ShouldReturnNull_WhenReservationBelongsToAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Balance = 10000
            };

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = anotherUserId,
                TotalPrice = 5000,
                Status = "Active"
            };

            _userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns(user);

            _reservationRepositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            // Act
            var result =
                _paymentService.MakePayment(
                    userId,
                    reservationId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MakePayment_ShouldThrowException_WhenReservationIsCancelled()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Balance = 10000
            };

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                TotalPrice = 5000,
                Status = "Cancelled"
            };

            _userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns(user);

            _reservationRepositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _paymentService.MakePayment(
                    userId,
                    reservationId));
        }

        [Fact]
        public void MakePayment_ShouldThrowException_WhenReservationIsAlreadyPaid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Balance = 10000
            };

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                TotalPrice = 5000,
                Status = "Active"
            };

            var existingPayment = new Payment
            {
                Id = Guid.NewGuid(),
                ReservationId = reservationId,
                Amount = 5000,
                PaymentType = "Payment",
                Status = "Completed"
            };

            _userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns(user);

            _reservationRepositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            _paymentRepositoryMock
                .Setup(x =>
                    x.GetPaymentByReservationId(reservationId))
                .Returns(existingPayment);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _paymentService.MakePayment(
                    userId,
                    reservationId));
        }

        [Fact]
        public void MakePayment_ShouldThrowException_WhenBalanceIsInsufficient()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Balance = 3000
            };

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                TotalPrice = 5000,
                Status = "Active"
            };

            _userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns(user);

            _reservationRepositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            _paymentRepositoryMock
                .Setup(x =>
                    x.GetPaymentByReservationId(reservationId))
                .Returns((Payment?)null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => _paymentService.MakePayment(
                    userId,
                    reservationId));
        }

        [Fact]
        public void MakePayment_ShouldCreatePaymentSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Balance = 10000
            };

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                TotalPrice = 5000,
                Status = "Active"
            };

            var paymentDto = new PaymentDto
            {
                Id = Guid.NewGuid(),
                ReservationId = reservationId,
                Amount = 5000,
                PaymentType = "Payment",
                Status = "Completed",
                TransactionDate = DateTime.UtcNow
            };

            _userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns(user);

            _reservationRepositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            _paymentRepositoryMock
                .Setup(x =>
                    x.GetPaymentByReservationId(reservationId))
                .Returns((Payment?)null);

            _mapperMock
                .Setup(x =>
                    x.Map<PaymentDto>(It.IsAny<Payment>()))
                .Returns(paymentDto);

            // Act
            var result =
                _paymentService.MakePayment(
                    userId,
                    reservationId);

            // Assert
            Assert.NotNull(result);

            _paymentRepositoryMock.Verify(
                x => x.Add(It.Is<Payment>(p =>
                    p.ReservationId == reservationId &&
                    p.Amount == 5000 &&
                    p.PaymentType == "Payment" &&
                    p.Status == "Completed" &&
                    p.IsDeleted == false)),
                Times.Once);

            _paymentRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.UpdateBalance(
                    userId,
                    -5000),
                Times.Once);

            _userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void GetPayments_ShouldReturnPayments()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();

            var payments = new List<Payment>
            {
                new Payment
                {
                    Id = Guid.NewGuid(),
                    ReservationId = Guid.NewGuid(),
                    Amount = 5000,
                    PaymentType = "Payment",
                    Status = "Completed"
                }
            };

            var paymentDtos = new List<PaymentDto>
            {
                new PaymentDto
                {
                    Id = payments[0].Id,
                    ReservationId =
                        payments[0].ReservationId,
                    Amount = 5000,
                    PaymentType = "Payment",
                    Status = "Completed"
                }
            };

            _paymentRepositoryMock
                .Setup(x =>
                    x.GetPayments(userId, hotelId))
                .Returns(payments);

            _mapperMock
                .Setup(x =>
                    x.Map<List<PaymentDto>>(
                        It.IsAny<List<Payment>>()))
                .Returns(paymentDtos);

            // Act
            var result =
                _paymentService.GetPayments(
                    userId,
                    hotelId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(5000, result[0].Amount);

            _paymentRepositoryMock.Verify(
                x => x.GetPayments(userId, hotelId),
                Times.Once);
        }

        [Fact]
        public void GetPayments_ShouldReturnEmptyList_WhenThereAreNoPayments()
        {
            // Arrange
            var userId = Guid.NewGuid();
            Guid? hotelId = null;

            var payments = new List<Payment>();
            var paymentDtos = new List<PaymentDto>();

            _paymentRepositoryMock
                .Setup(x =>
                    x.GetPayments(userId, hotelId))
                .Returns(payments);

            _mapperMock
                .Setup(x =>
                    x.Map<List<PaymentDto>>(
                        It.IsAny<List<Payment>>()))
                .Returns(paymentDtos);

            // Act
            var result =
                _paymentService.GetPayments(
                    userId,
                    hotelId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}