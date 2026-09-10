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
    public class RoomServiceTests
    {
        private ErrorMessageService CreateErrorMessageService()
        {
            return new ErrorMessageService(
                "errorMessages.tr.json",
                "errorMessages.en.json",
                new HttpContextAccessor());
        }

        [Fact]
        public void GetRooms_ShouldReturnRooms()
        {
            // Arrange
            var hotelId = Guid.NewGuid();

            var rooms = new List<Room>
            {
                new Room
                {
                    Id = Guid.NewGuid(),
                    HotelId = hotelId,
                    RoomNumber = 101,
                    RoomType = "Standart",
                    Capacity = 2,
                    PricePerNight = 2500,
                    IsAvailable = true,
                    IsDeleted = false
                }
            };

            var roomDtos = new List<RoomDto>
            {
                new RoomDto
                {
                    Id = rooms[0].Id,
                    HotelId = hotelId,
                    RoomNumber = 101,
                    RoomType = "Standart",
                    Capacity = 2,
                    PricePerNight = 2500,
                    IsAvailable = true
                }
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetRooms(hotelId))
                .Returns(rooms);

            mapperMock
                .Setup(x => x.Map<List<RoomDto>>(rooms))
                .Returns(roomDtos);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.GetRooms(hotelId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(101, result[0].RoomNumber);
            Assert.Equal("Standart", result[0].RoomType);

            repositoryMock.Verify(
                x => x.GetRooms(hotelId),
                Times.Once);

            mapperMock.Verify(
                x => x.Map<List<RoomDto>>(rooms),
                Times.Once);
        }

        [Fact]
        public void GetRooms_ShouldReturnAllRooms_WhenHotelIdIsNull()
        {
            // Arrange
            var rooms = new List<Room>
            {
                new Room
                {
                    Id = Guid.NewGuid(),
                    HotelId = Guid.NewGuid(),
                    RoomNumber = 101,
                    RoomType = "Standart",
                    Capacity = 2,
                    PricePerNight = 2500,
                    IsAvailable = true,
                    IsDeleted = false
                },
                new Room
                {
                    Id = Guid.NewGuid(),
                    HotelId = Guid.NewGuid(),
                    RoomNumber = 202,
                    RoomType = "Suit",
                    Capacity = 4,
                    PricePerNight = 5000,
                    IsAvailable = true,
                    IsDeleted = false
                }
            };

            var roomDtos = new List<RoomDto>
            {
                new RoomDto
                {
                    Id = rooms[0].Id,
                    HotelId = rooms[0].HotelId,
                    RoomNumber = 101,
                    RoomType = "Standart",
                    Capacity = 2,
                    PricePerNight = 2500,
                    IsAvailable = true
                },
                new RoomDto
                {
                    Id = rooms[1].Id,
                    HotelId = rooms[1].HotelId,
                    RoomNumber = 202,
                    RoomType = "Suit",
                    Capacity = 4,
                    PricePerNight = 5000,
                    IsAvailable = true
                }
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetRooms(null))
                .Returns(rooms);

            mapperMock
                .Setup(x => x.Map<List<RoomDto>>(rooms))
                .Returns(roomDtos);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.GetRooms(null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            repositoryMock.Verify(
                x => x.GetRooms(null),
                Times.Once);
        }

        [Fact]
        public void CreateRoom_ShouldCreateRoomSuccessfully_WhenHotelBelongsToUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();

            var hotel = new Hotel
            {
                Id = hotelId,
                UserId = userId,
                Name = "Grand Hotel",
                Address = "Atatürk Caddesi",
                City = "Bursa",
                IsDeleted = false
            };

            var roomDto = new RoomDto
            {
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500
            };

            var room = new Room
            {
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500
            };

            var resultDto = new RoomDto
            {
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500,
                IsAvailable = true
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            hotelRepositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            mapperMock
                .Setup(x => x.Map<Room>(roomDto))
                .Returns(room);

            mapperMock
                .Setup(x => x.Map<RoomDto>(room))
                .Returns(resultDto);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CreateRoom(
                userId,
                roomDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(101, result.RoomNumber);
            Assert.Equal("Standart", result.RoomType);

            Assert.NotEqual(Guid.Empty, room.Id);
            Assert.Equal(hotelId, room.HotelId);
            Assert.False(room.IsDeleted);
            Assert.True(room.IsAvailable);

            hotelRepositoryMock.Verify(
                x => x.GetById(hotelId),
                Times.Once);

            repositoryMock.Verify(
                x => x.Add(room),
                Times.Once);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void CreateRoom_ShouldReturnNull_WhenHotelDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();

            var roomDto = new RoomDto
            {
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            hotelRepositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns((Hotel?)null);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CreateRoom(
                userId,
                roomDto);

            // Assert
            Assert.Null(result);

            repositoryMock.Verify(
                x => x.Add(It.IsAny<Room>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);

            mapperMock.Verify(
                x => x.Map<Room>(It.IsAny<RoomDto>()),
                Times.Never);
        }

        [Fact]
        public void CreateRoom_ShouldReturnNull_WhenHotelBelongsToAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();

            var hotel = new Hotel
            {
                Id = hotelId,
                UserId = anotherUserId,
                Name = "Başkasının Oteli",
                Address = "Adres",
                City = "Bursa",
                IsDeleted = false
            };

            var roomDto = new RoomDto
            {
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            hotelRepositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CreateRoom(
                userId,
                roomDto);

            // Assert
            Assert.Null(result);

            repositoryMock.Verify(
                x => x.Add(It.IsAny<Room>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);

            mapperMock.Verify(
                x => x.Map<Room>(It.IsAny<RoomDto>()),
                Times.Never);
        }

        [Fact]
        public void UpdateRoom_ShouldUpdateRoomSuccessfully_WhenUserOwnsHotel()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var room = new Room
            {
                Id = roomId,
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var hotel = new Hotel
            {
                Id = hotelId,
                UserId = userId,
                Name = "Grand Hotel",
                Address = "Adres",
                City = "Bursa",
                IsDeleted = false
            };

            var updatedDto = new RoomDto
            {
                HotelId = hotelId,
                RoomNumber = 102,
                RoomType = "Suit",
                Capacity = 4,
                PricePerNight = 4000,
                IsAvailable = true
            };

            var resultDto = new RoomDto
            {
                Id = roomId,
                HotelId = hotelId,
                RoomNumber = 102,
                RoomType = "Suit",
                Capacity = 4,
                PricePerNight = 4000,
                IsAvailable = true
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            hotelRepositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            mapperMock
                .Setup(x => x.Map(updatedDto, room))
                .Callback<RoomDto, Room>((dto, entity) =>
                {
                    entity.RoomNumber = dto.RoomNumber;
                    entity.RoomType = dto.RoomType;
                    entity.Capacity = dto.Capacity;
                    entity.PricePerNight = dto.PricePerNight;
                    entity.IsAvailable = dto.IsAvailable;
                });

            mapperMock
                .Setup(x => x.Map<RoomDto>(room))
                .Returns(resultDto);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.UpdateRoom(
                userId,
                roomId,
                updatedDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(102, room.RoomNumber);
            Assert.Equal("Suit", room.RoomType);
            Assert.Equal(4, room.Capacity);
            Assert.Equal(4000, room.PricePerNight);

            repositoryMock.Verify(
                x => x.GetById(roomId),
                Times.Once);

            hotelRepositoryMock.Verify(
                x => x.GetById(hotelId),
                Times.Once);

            repositoryMock.Verify(
                x => x.Update(room),
                Times.Once);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void UpdateRoom_ShouldReturnNull_WhenRoomDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns((Room?)null);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.UpdateRoom(
                userId,
                roomId,
                new RoomDto
                {
                    RoomNumber = 102,
                    RoomType = "Suit",
                    Capacity = 4,
                    PricePerNight = 4000
                });

            // Assert
            Assert.Null(result);

            hotelRepositoryMock.Verify(
                x => x.GetById(It.IsAny<Guid>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Room>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void UpdateRoom_ShouldThrowUnauthorized_WhenHotelBelongsToAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var room = new Room
            {
                Id = roomId,
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var hotel = new Hotel
            {
                Id = hotelId,
                UserId = anotherUserId,
                Name = "Başkasının Oteli",
                Address = "Adres",
                City = "Bursa",
                IsDeleted = false
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            hotelRepositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
                service.UpdateRoom(
                    userId,
                    roomId,
                    new RoomDto
                    {
                        RoomNumber = 102,
                        RoomType = "Suit",
                        Capacity = 4,
                        PricePerNight = 4000
                    }));

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Room>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);

            mapperMock.Verify(
                x => x.Map(
                    It.IsAny<RoomDto>(),
                    It.IsAny<Room>()),
                Times.Never);
        }

        [Fact]
        public void UpdateRoom_ShouldThrowUnauthorized_WhenHotelDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var room = new Room
            {
                Id = roomId,
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            hotelRepositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns((Hotel?)null);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
                service.UpdateRoom(
                    userId,
                    roomId,
                    new RoomDto
                    {
                        RoomNumber = 102,
                        RoomType = "Suit",
                        Capacity = 4,
                        PricePerNight = 4000
                    }));

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Room>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void DeleteRoom_ShouldReturnTrue_WhenUserOwnsHotel()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var room = new Room
            {
                Id = roomId,
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var hotel = new Hotel
            {
                Id = hotelId,
                UserId = userId,
                Name = "Grand Hotel",
                Address = "Adres",
                City = "Bursa",
                IsDeleted = false
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            hotelRepositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.DeleteRoom(
                userId,
                roomId);

            // Assert
            Assert.True(result);
            Assert.True(room.IsDeleted);

            repositoryMock.Verify(
                x => x.GetById(roomId),
                Times.Once);

            hotelRepositoryMock.Verify(
                x => x.GetById(hotelId),
                Times.Once);

            repositoryMock.Verify(
                x => x.Update(room),
                Times.Once);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void DeleteRoom_ShouldReturnFalse_WhenRoomDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns((Room?)null);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.DeleteRoom(
                userId,
                roomId);

            // Assert
            Assert.False(result);

            hotelRepositoryMock.Verify(
                x => x.GetById(It.IsAny<Guid>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Room>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void DeleteRoom_ShouldThrowUnauthorized_WhenHotelBelongsToAnotherUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anotherUserId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var room = new Room
            {
                Id = roomId,
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var hotel = new Hotel
            {
                Id = hotelId,
                UserId = anotherUserId,
                Name = "Başkasının Oteli",
                Address = "Adres",
                City = "Bursa",
                IsDeleted = false
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            hotelRepositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
                service.DeleteRoom(
                    userId,
                    roomId));

            Assert.False(room.IsDeleted);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Room>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void DeleteRoom_ShouldThrowUnauthorized_WhenHotelDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();
            var roomId = Guid.NewGuid();

            var room = new Room
            {
                Id = roomId,
                HotelId = hotelId,
                RoomNumber = 101,
                RoomType = "Standart",
                Capacity = 2,
                PricePerNight = 2500,
                IsAvailable = true,
                IsDeleted = false
            };

            var repositoryMock = new Mock<IRoomRepository>();
            var hotelRepositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(roomId))
                .Returns(room);

            hotelRepositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns((Hotel?)null);

            var service = new RoomService(
                repositoryMock.Object,
                hotelRepositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
                service.DeleteRoom(
                    userId,
                    roomId));

            Assert.False(room.IsDeleted);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Room>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }
    }
}