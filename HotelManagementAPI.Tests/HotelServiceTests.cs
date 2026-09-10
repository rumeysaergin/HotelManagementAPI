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
    public class HotelServiceTests
    {
        private ErrorMessageService CreateErrorMessageService()
        {
            return new ErrorMessageService(
                "errorMessages.tr.json",
                "errorMessages.en.json",
                new HttpContextAccessor());
        }

        [Fact]
        public void GetHotels_ShouldReturnHotelsForUser()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var hotels = new List<Hotel>
            {
                new Hotel
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Name = "Grand Hotel",
                    Address = "Atatürk Caddesi",
                    City = "Bursa",
                    IsDeleted = false
                }
            };

            var hotelDtos = new List<HotelDto>
            {
                new HotelDto
                {
                    Id = hotels[0].Id,
                    UserId = userId,
                    Name = "Grand Hotel",
                    Address = "Atatürk Caddesi",
                    City = "Bursa"
                }
            };

            var repositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetHotels(userId))
                .Returns(hotels);

            mapperMock
                .Setup(x => x.Map<List<HotelDto>>(hotels))
                .Returns(hotelDtos);

            var service = new HotelService(
                repositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.GetHotels(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Grand Hotel", result[0].Name);

            repositoryMock.Verify(
                x => x.GetHotels(userId),
                Times.Once);

            mapperMock.Verify(
                x => x.Map<List<HotelDto>>(hotels),
                Times.Once);
        }

        [Fact]
        public void CreateHotel_ShouldCreateHotelSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var hotelDto = new HotelDto
            {
                Name = "Grand Hotel",
                Address = "Atatürk Caddesi",
                City = "Bursa"
            };

            var hotel = new Hotel
            {
                Name = hotelDto.Name,
                Address = hotelDto.Address,
                City = hotelDto.City
            };

            var resultDto = new HotelDto
            {
                Name = "Grand Hotel",
                Address = "Atatürk Caddesi",
                City = "Bursa",
                UserId = userId
            };

            var repositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            mapperMock
                .Setup(x => x.Map<Hotel>(hotelDto))
                .Returns(hotel);

            mapperMock
                .Setup(x => x.Map<HotelDto>(hotel))
                .Returns(resultDto);

            var service = new HotelService(
                repositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.CreateHotel(userId, hotelDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Grand Hotel", result.Name);

            Assert.NotEqual(Guid.Empty, hotel.Id);
            Assert.Equal(userId, hotel.UserId);
            Assert.False(hotel.IsDeleted);

            repositoryMock.Verify(
                x => x.Add(hotel),
                Times.Once);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void UpdateHotel_ShouldUpdateHotelSuccessfully_WhenUserOwnsHotel()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();

            var hotel = new Hotel
            {
                Id = hotelId,
                UserId = userId,
                Name = "Eski Hotel",
                Address = "Eski Adres",
                City = "Bursa",
                IsDeleted = false
            };

            var updatedDto = new HotelDto
            {
                Name = "Yeni Hotel",
                Address = "Yeni Adres",
                City = "Istanbul"
            };

            var resultDto = new HotelDto
            {
                Id = hotelId,
                UserId = userId,
                Name = "Yeni Hotel",
                Address = "Yeni Adres",
                City = "Istanbul"
            };

            var repositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            mapperMock
                .Setup(x => x.Map(updatedDto, hotel))
                .Callback<HotelDto, Hotel>((dto, entity) =>
                {
                    entity.Name = dto.Name;
                    entity.Address = dto.Address;
                    entity.City = dto.City;
                });

            mapperMock
                .Setup(x => x.Map<HotelDto>(hotel))
                .Returns(resultDto);

            var service = new HotelService(
                repositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.UpdateHotel(
                userId,
                hotelId,
                updatedDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Yeni Hotel", hotel.Name);
            Assert.Equal("Yeni Adres", hotel.Address);
            Assert.Equal("Istanbul", hotel.City);

            repositoryMock.Verify(
                x => x.GetById(hotelId),
                Times.Once);

            repositoryMock.Verify(
                x => x.Update(hotel),
                Times.Once);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void UpdateHotel_ShouldReturnNull_WhenHotelDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();

            var repositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns((Hotel?)null);

            var service = new HotelService(
                repositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.UpdateHotel(
                userId,
                hotelId,
                new HotelDto
                {
                    Name = "Yeni Hotel",
                    Address = "Yeni Adres",
                    City = "Bursa"
                });

            // Assert
            Assert.Null(result);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Hotel>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);

            mapperMock.Verify(
                x => x.Map(
                    It.IsAny<HotelDto>(),
                    It.IsAny<Hotel>()),
                Times.Never);
        }

        [Fact]
        public void UpdateHotel_ShouldThrowUnauthorized_WhenHotelBelongsToAnotherUser()
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

            var repositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            var service = new HotelService(
                repositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
                service.UpdateHotel(
                    userId,
                    hotelId,
                    new HotelDto
                    {
                        Name = "Değiştirmeye Çalışılan Otel",
                        Address = "Yeni Adres",
                        City = "Istanbul"
                    }));

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Hotel>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);

            mapperMock.Verify(
                x => x.Map(
                    It.IsAny<HotelDto>(),
                    It.IsAny<Hotel>()),
                Times.Never);
        }

        [Fact]
        public void DeleteHotel_ShouldReturnTrue_WhenUserOwnsHotel()
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

            var repositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            var service = new HotelService(
                repositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.DeleteHotel(
                userId,
                hotelId);

            // Assert
            Assert.True(result);
            Assert.True(hotel.IsDeleted);

            repositoryMock.Verify(
                x => x.GetById(hotelId),
                Times.Once);

            repositoryMock.Verify(
                x => x.Update(hotel),
                Times.Once);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void DeleteHotel_ShouldReturnFalse_WhenHotelDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var hotelId = Guid.NewGuid();

            var repositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns((Hotel?)null);

            var service = new HotelService(
                repositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act
            var result = service.DeleteHotel(
                userId,
                hotelId);

            // Assert
            Assert.False(result);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Hotel>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }

        [Fact]
        public void DeleteHotel_ShouldThrowUnauthorized_WhenHotelBelongsToAnotherUser()
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

            var repositoryMock = new Mock<IHotelRepository>();
            var mapperMock = new Mock<IMapper>();

            repositoryMock
                .Setup(x => x.GetById(hotelId))
                .Returns(hotel);

            var service = new HotelService(
                repositoryMock.Object,
                mapperMock.Object,
                CreateErrorMessageService());

            // Act & Assert
            Assert.Throws<UnauthorizedAccessException>(() =>
                service.DeleteHotel(
                    userId,
                    hotelId));

            Assert.False(hotel.IsDeleted);

            repositoryMock.Verify(
                x => x.Update(It.IsAny<Hotel>()),
                Times.Never);

            repositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }
    }
}