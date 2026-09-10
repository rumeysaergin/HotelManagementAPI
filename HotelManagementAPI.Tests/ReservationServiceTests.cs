using AutoMapper;
using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Application.Services;
using HotelManagementAPI.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace HotelManagementAPI.Tests
{
    public class ReservationServiceTests
    {
        private ErrorMessageService CreateErrorMessageService()
        {
            return new ErrorMessageService(
                "errorMessages.tr.json",
                "errorMessages.en.json",
                new HttpContextAccessor());
        }

        [Fact]
        public void GetReservations_ShouldReturnReservationsForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var reservations = new List<Reservation>
            {
                new Reservation
                {
                    Id = reservationId,
                    UserId = userId,
                    RoomId = roomId,
                    CheckInDate = DateTime.Today.AddDays(5),
                    CheckOutDate = DateTime.Today.AddDays(8),
                    TotalPrice = 7500,
                    Status = "Active",
                    CancellationFee = 0,
                    IsDeleted = false
                }
            };

            var reservationDtos = new List<ReservationDto>
            {
                new ReservationDto
                {
                    Id = reservationId,
                    UserId = userId,
                    RoomId = roomId,
                    CheckInDate = DateTime.Today.AddDays(5),
                    CheckOutDate = DateTime.Today.AddDays(8),
                    TotalPrice = 7500,
                    Status = "Active",
                    CancellationFee = 0
                }
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetReservations(userId, hotelId))
                .Returns(reservations);

            mapperMock
                .Setup(x => x.Map<List<ReservationDto>>(reservations))
                .Returns(reservationDtos);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.GetReservations(userId, hotelId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(reservationId, result[0].Id);
            Assert.Equal(7500, result[0].TotalPrice);

            repositoryMock.Verify(
                x => x.GetReservations(userId, hotelId),
                Times.Once);

            mapperMock.Verify(
                x => x.Map<List<ReservationDto>>(reservations),
                Times.Once);
        }

        [Fact]
        public void CreateReservation_ShouldReturnNull_WhenRoomDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var reservationDto = new ReservationDto
            {
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(5),
                CheckOutDate = DateTime.Today.AddDays(8)
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns((Room?)null);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CreateReservation(
                userId,
                reservationDto);

            // Assert
            Assert.Null(result);

            repositoryMock.Verify(
                x => x.Add(It.IsAny<Reservation>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void CreateReservation_ShouldThrowArgumentException_WhenDatesAreInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var room = new Room
            {
                Id = roomId,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var reservationDto = new ReservationDto
            {
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(8),
                CheckOutDate = DateTime.Today.AddDays(5)
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.CreateReservation(
                    userId,
                    reservationDto));

            repositoryMock.Verify(
                x => x.Add(It.IsAny<Reservation>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void CreateReservation_ShouldThrowArgumentException_WhenCheckInDateIsInThePast()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var room = new Room
            {
                Id = roomId,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var reservationDto = new ReservationDto
            {
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(-1),
                CheckOutDate = DateTime.Today.AddDays(2)
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.CreateReservation(
                    userId,
                    reservationDto));

            repositoryMock.Verify(
                x => x.Add(It.IsAny<Reservation>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void CreateReservation_ShouldThrowInvalidOperationException_WhenThereIsConflict()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var room = new Room
            {
                Id = roomId,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var reservationDto = new ReservationDto
            {
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(5),
                CheckOutDate = DateTime.Today.AddDays(8)
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            repositoryMock
                .Setup(x => x.HasConflict(
                    roomId,
                    reservationDto.CheckInDate,
                    reservationDto.CheckOutDate))
                .Returns(true);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.CreateReservation(
                    userId,
                    reservationDto));

            repositoryMock.Verify(
                x => x.Add(It.IsAny<Reservation>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void CreateReservation_ShouldCreateReservationSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var checkIn = DateTime.Today.AddDays(5);
            var checkOut = DateTime.Today.AddDays(8);

            var room = new Room
            {
                Id = roomId,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var reservationDto = new ReservationDto
            {
                RoomId = roomId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut
            };

            var reservation = new Reservation
            {
                RoomId = roomId
            };

            var resultDto = new ReservationDto
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                RoomId = roomId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                TotalPrice = 7500,
                Status = "Active",
                CancellationFee = 0
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            repositoryMock
                .Setup(x => x.HasConflict(
                    roomId,
                    checkIn,
                    checkOut))
                .Returns(false);

            mapperMock
                .Setup(x => x.Map<Reservation>(reservationDto))
                .Returns(reservation);

            mapperMock
                .Setup(x => x.Map<ReservationDto>(reservation))
                .Returns(resultDto);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CreateReservation(
                userId,
                reservationDto);

            // Assert
            Assert.NotNull(result);

            Assert.Equal(userId, reservation.UserId);
            Assert.Equal(7500, reservation.TotalPrice);
            Assert.Equal("Active", reservation.Status);
            Assert.Equal(0, reservation.CancellationFee);
            Assert.False(reservation.IsDeleted);

            Assert.False(room.IsAvailable);

            Assert.NotEqual(Guid.Empty, reservation.Id);

            repositoryMock.Verify(
                x => x.Add(reservation),
                Times.Once);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void UpdateReservation_ShouldReturnNull_WhenReservationDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns((Reservation?)null);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.UpdateReservation(
                userId,
                reservationId,
                new ReservationDto());

            // Assert
            Assert.Null(result);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Reservation>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void UpdateReservation_ShouldReturnNull_WhenReservationBelongsToAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = anotherUserId
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.UpdateReservation(
                userId,
                reservationId,
                new ReservationDto());

            // Assert
            Assert.Null(result);

            repositoryMock.Verify(
                x => x.HasPayment(It.IsAny<Guid>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Reservation>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void UpdateReservation_ShouldThrowException_WhenReservationHasPayment()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                RoomId = Guid.NewGuid()
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            repositoryMock
                .Setup(x => x.HasPayment(reservationId))
                .Returns(true);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.UpdateReservation(
                    userId,
                    reservationId,
                    new ReservationDto()));

            roomRepositoryMock.Verify(
                x => x.GetById(It.IsAny<Guid>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Reservation>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void UpdateReservation_ShouldThrowException_WhenDatesAreInvalid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                RoomId = roomId
            };

            var room = new Room
            {
                Id = roomId,
                PricePerNight = 2500
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            repositoryMock
                .Setup(x => x.HasPayment(reservationId))
                .Returns(false);

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            var updatedDto = new ReservationDto
            {
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(8),
                CheckOutDate = DateTime.Today.AddDays(5)
            };

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                service.UpdateReservation(
                    userId,
                    reservationId,
                    updatedDto));

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Reservation>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void UpdateReservation_ShouldThrowException_WhenThereIsConflict()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                RoomId = roomId
            };

            var room = new Room
            {
                Id = roomId,
                PricePerNight = 2500
            };

            var updatedDto = new ReservationDto
            {
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(5),
                CheckOutDate = DateTime.Today.AddDays(8)
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            repositoryMock
                .Setup(x => x.HasPayment(reservationId))
                .Returns(false);

            repositoryMock
                .Setup(x => x.HasConflict(
                    roomId,
                    updatedDto.CheckInDate,
                    updatedDto.CheckOutDate,
                    reservationId))
                .Returns(true);

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.UpdateReservation(
                    userId,
                    reservationId,
                    updatedDto));

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Reservation>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void UpdateReservation_ShouldUpdateSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(5),
                CheckOutDate = DateTime.Today.AddDays(7),
                TotalPrice = 5000,
                Status = "Active"
            };

            var room = new Room
            {
                Id = roomId,
                PricePerNight = 3000,
                IsAvailable = false
            };

            var updatedDto = new ReservationDto
            {
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(10),
                CheckOutDate = DateTime.Today.AddDays(12)
            };

            var resultDto = new ReservationDto
            {
                Id = reservationId,
                UserId = userId,
                RoomId = roomId,
                CheckInDate = updatedDto.CheckInDate,
                CheckOutDate = updatedDto.CheckOutDate,
                TotalPrice = 6000,
                Status = "Active"
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            repositoryMock
                .Setup(x => x.HasPayment(reservationId))
                .Returns(false);

            repositoryMock
                .Setup(x => x.HasConflict(
                    roomId,
                    updatedDto.CheckInDate,
                    updatedDto.CheckOutDate,
                    reservationId))
                .Returns(false);

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            mapperMock
                .Setup(x => x.Map(updatedDto, reservation))
                .Callback<ReservationDto, Reservation>(
                    (dto, entity) =>
                    {
                        entity.RoomId = dto.RoomId;
                        entity.CheckInDate = dto.CheckInDate;
                        entity.CheckOutDate = dto.CheckOutDate;
                    });

            mapperMock
                .Setup(x => x.Map<ReservationDto>(reservation))
                .Returns(resultDto);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.UpdateReservation(
                userId,
                reservationId,
                updatedDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6000, reservation.TotalPrice);
            Assert.Equal(updatedDto.CheckInDate, reservation.CheckInDate);
            Assert.Equal(updatedDto.CheckOutDate, reservation.CheckOutDate);

            Assert.False(room.IsAvailable);

            repositoryMock.Verify(
                x => x.Update(reservation),
                Times.Once);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void CancelReservation_ShouldReturnNull_WhenReservationDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns((Reservation?)null);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CancelReservation(
                userId,
                reservationId);

            // Assert
            Assert.Null(result);

            roomRepositoryMock.Verify(
                x => x.GetById(It.IsAny<Guid>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Reservation>()),
                Times.Never);
        }

        [Fact]
        public void CancelReservation_ShouldReturnNull_WhenReservationBelongsToAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = anotherUserId,
                Status = "Active"
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CancelReservation(
                userId,
                reservationId);

            // Assert
            Assert.Null(result);

            roomRepositoryMock.Verify(
                x => x.GetById(It.IsAny<Guid>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Reservation>()),
                Times.Never);
        }

        [Fact]
        public void CancelReservation_ShouldThrowException_WhenAlreadyCancelled()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                Status = "Cancelled"
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.CancelReservation(
                    userId,
                    reservationId));

            roomRepositoryMock.Verify(
                x => x.GetById(It.IsAny<Guid>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Reservation>()),
                Times.Never);
        }

        [Fact]
        public void CancelReservation_ShouldApplyZeroFee_WhenCheckInIsAtLeastTwoDaysAway()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(5),
                CheckOutDate = DateTime.Today.AddDays(8),
                TotalPrice = 7500,
                Status = "Active",
                CancellationFee = 0
            };

            var room = new Room
            {
                Id = roomId,
                IsAvailable = false
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            paymentRepositoryMock
                .Setup(x => x.GetPaymentByReservationId(reservationId))
                .Returns((Payment?)null);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CancelReservation(
                userId,
                reservationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, reservation.CancellationFee);
            Assert.Equal("Cancelled", reservation.Status);
            Assert.True(room.IsAvailable);

            repositoryMock.Verify(
                x => x.Update(reservation),
                Times.Once);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);

            userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);

            paymentRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void CancelReservation_ShouldApplyThirtyPercentFee_WhenCheckInIsOneDayAway()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                RoomId = roomId,
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(3),
                TotalPrice = 10000,
                Status = "Active"
            };

            var room = new Room
            {
                Id = roomId,
                IsAvailable = false
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            paymentRepositoryMock
                .Setup(x => x.GetPaymentByReservationId(reservationId))
                .Returns((Payment?)null);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CancelReservation(
                userId,
                reservationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3000, reservation.CancellationFee);
            Assert.Equal("Cancelled", reservation.Status);
            Assert.True(room.IsAvailable);
        }

        [Fact]
        public void CancelReservation_ShouldApplySixtyPercentFee_WhenCheckInIsTodayOrPast()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var reservationId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                RoomId = roomId,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(2),
                TotalPrice = 10000,
                Status = "Active"
            };

            var room = new Room
            {
                Id = roomId,
                IsAvailable = false
            };

            var repositoryMock = new Mock<IReservationRepository>();
            var roomRepositoryMock = new Mock<IRoomRepository>();
            var userRepositoryMock = new Mock<IUserRepository>();
            var paymentRepositoryMock = new Mock<IPaymentRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(reservationId))
                .Returns(reservation);

            roomRepositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            paymentRepositoryMock
                .Setup(x => x.GetPaymentByReservationId(reservationId))
                .Returns((Payment?)null);

            var service = new ReservationService(
                repositoryMock.Object,
                roomRepositoryMock.Object,
                userRepositoryMock.Object,
                paymentRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CancelReservation(
                userId,
                reservationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6000, reservation.CancellationFee);
            Assert.Equal("Cancelled", reservation.Status);
            Assert.True(room.IsAvailable);
        }
    }
}