using AutoMapper;
using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Application.Services;
using HotelManagementAPI.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace HotelManagementAPI.Tests
{
    public class UserServiceTests
    {
        [Fact]
        public void Register_ShouldCreateUserSuccessfully()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();
            var configurationMock = new Mock<IConfiguration>();

            var errorMessageService =
                new ErrorMessageService(
                    "errorMessages.tr.json",
                    "errorMessages.en.json",
                    new HttpContextAccessor());

            var userDto = new UserDto
            {
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com",
                Password = "12345678"
            };

            var user = new User
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email
            };

            userRepositoryMock
                .Setup(x => x.EmailExists(userDto.Email))
                .Returns(false);

            mapperMock
                .Setup(x => x.Map<User>(userDto))
                .Returns(user);

            mapperMock
                .Setup(x => x.Map<UserDto>(It.IsAny<User>()))
                .Returns(userDto);

            var userService = new UserService(
                userRepositoryMock.Object,
                mapperMock.Object,
                configurationMock.Object,
                errorMessageService);

            // Act
            var result = userService.Register(userDto);

            // Assert
            Assert.NotNull(result);

            userRepositoryMock.Verify(
                x => x.Add(It.IsAny<User>()),
                Times.Once);

            userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }

        [Fact]
        public void Register_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();
            var configurationMock = new Mock<IConfiguration>();

            var errorMessageService =
                new ErrorMessageService(
                    "errorMessages.tr.json",
                    "errorMessages.en.json",
                    new HttpContextAccessor());

            var userDto = new UserDto
            {
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com",
                Password = "12345678"
            };

            userRepositoryMock
                .Setup(x => x.EmailExists(userDto.Email))
                .Returns(true);

            var userService = new UserService(
                userRepositoryMock.Object,
                mapperMock.Object,
                configurationMock.Object,
                errorMessageService);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => userService.Register(userDto));

            userRepositoryMock.Verify(
                x => x.Add(It.IsAny<User>()),
                Times.Never);

            userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }
        [Fact]
        public void Login_ShouldReturnToken_WhenCredentialsAreCorrect()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();

            var configurationData = new Dictionary<string, string?>
    {
        { "Jwt:Key", "HotelManagementAPI-TestSecretKey-123456789" },
        { "Jwt:Issuer", "HotelManagementAPI" },
        { "Jwt:Audience", "HotelManagementAPIUsers" },
        { "Jwt:ExpireMinutes", "60" }
    };

            var configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(configurationData)
                    .Build();

            var errorMessageService =
                new ErrorMessageService(
                    "errorMessages.tr.json",
                    "errorMessages.en.json",
                    new HttpContextAccessor());

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com"
            };

            var passwordHasher = new PasswordHasher<User>();

            user.Password = passwordHasher.HashPassword(
                user,
                "12345678");

            userRepositoryMock
                .Setup(x => x.GetByEmail("ahmet@test.com"))
                .Returns(user);

            var userService = new UserService(
                userRepositoryMock.Object,
                mapperMock.Object,
                configuration,
                errorMessageService);

            // Act
            var token = userService.Login(
                "ahmet@test.com",
                "12345678");

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }
        [Fact]
        public void Login_ShouldThrowException_WhenPasswordIsWrong()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();

            var configurationData = new Dictionary<string, string?>
    {
        { "Jwt:Key", "HotelManagementAPI-TestSecretKey-123456789" },
        { "Jwt:Issuer", "HotelManagementAPI" },
        { "Jwt:Audience", "HotelManagementAPIUsers" },
        { "Jwt:ExpireMinutes", "60" }
    };

            var configuration =
                new ConfigurationBuilder()
                    .AddInMemoryCollection(configurationData)
                    .Build();

            var errorMessageService =
                new ErrorMessageService(
                    "errorMessages.tr.json",
                    "errorMessages.en.json",
                    new HttpContextAccessor());

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com"
            };

            var passwordHasher = new PasswordHasher<User>();

            user.Password = passwordHasher.HashPassword(
                user,
                "12345678");

            userRepositoryMock
                .Setup(x => x.GetByEmail("ahmet@test.com"))
                .Returns(user);

            var userService = new UserService(
                userRepositoryMock.Object,
                mapperMock.Object,
                configuration,
                errorMessageService);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () => userService.Login(
                    "ahmet@test.com",
                    "yanlis-sifre"));
        }
        [Fact]
        public void GetProfile_ShouldReturnUserProfile_WhenUserExists()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();
            var configurationMock = new Mock<IConfiguration>();

            var errorMessageService =
                new ErrorMessageService(
                    "errorMessages.tr.json",
                    "errorMessages.en.json",
                    new HttpContextAccessor());

            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com"
            };

            var userDto = new UserDto
            {
                Id = userId,
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com"
            };

            userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns(user);

            mapperMock
                .Setup(x => x.Map<UserDto>(user))
                .Returns(userDto);

            var userService = new UserService(
                userRepositoryMock.Object,
                mapperMock.Object,
                configurationMock.Object,
                errorMessageService);

            // Act
            var result = userService.GetProfile(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("Ahmet", result.FirstName);
            Assert.Equal("Yılmaz", result.LastName);
            Assert.Equal("ahmet@test.com", result.Email);
        }
        [Fact]
        public void GetProfile_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();
            var configurationMock = new Mock<IConfiguration>();

            var errorMessageService =
                new ErrorMessageService(
                    "errorMessages.tr.json",
                    "errorMessages.en.json",
                    new HttpContextAccessor());

            var userId = Guid.NewGuid();

            userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns((User?)null);

            var userService = new UserService(
                userRepositoryMock.Object,
                mapperMock.Object,
                configurationMock.Object,
                errorMessageService);

            // Act
            var result = userService.GetProfile(userId);

            // Assert
            Assert.Null(result);

            mapperMock.Verify(
                x => x.Map<UserDto>(It.IsAny<User>()),
                Times.Never);
        }
        [Fact]
        public void UpdateUser_ShouldUpdateUserSuccessfully_WhenUserExists()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();
            var configurationMock = new Mock<IConfiguration>();

            var errorMessageService =
                new ErrorMessageService(
                    "errorMessages.tr.json",
                    "errorMessages.en.json",
                    new HttpContextAccessor());

            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "old@test.com",
                Password = "old-hashed-password"
            };

            var updatedUserDto = new UserDto
            {
                FirstName = "Mehmet",
                LastName = "Kaya",
                Email = "new@test.com",
                Password = ""
            };

            var resultDto = new UserDto
            {
                Id = userId,
                FirstName = "Mehmet",
                LastName = "Kaya",
                Email = "new@test.com",
                Password = ""
            };

            userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns(user);

            mapperMock
                .Setup(x => x.Map<UserDto>(user))
                .Returns(resultDto);

            var userService = new UserService(
                userRepositoryMock.Object,
                mapperMock.Object,
                configurationMock.Object,
                errorMessageService);

            // Act
            var result = userService.UpdateUser(
                userId,
                updatedUserDto);

            // Assert
            Assert.NotNull(result);

            Assert.Equal("Mehmet", result.FirstName);
            Assert.Equal("Kaya", result.LastName);
            Assert.Equal("new@test.com", result.Email);

            Assert.Equal("Mehmet", user.FirstName);
            Assert.Equal("Kaya", user.LastName);
            Assert.Equal("new@test.com", user.Email);

            userRepositoryMock.Verify(
                x => x.Update(user),
                Times.Once);

            userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }
        [Fact]
        public void UpdateUser_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();
            var configurationMock = new Mock<IConfiguration>();

            var errorMessageService =
                new ErrorMessageService(
                    "errorMessages.tr.json",
                    "errorMessages.en.json",
                    new HttpContextAccessor());

            var userId = Guid.NewGuid();

            userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns((User?)null);

            var userService = new UserService(
                userRepositoryMock.Object,
                mapperMock.Object,
                configurationMock.Object,
                errorMessageService);

            // Act
            var result = userService.UpdateUser(
                userId,
                new UserDto
                {
                    FirstName = "Mehmet",
                    LastName = "Kaya",
                    Email = "new@test.com",
                    Password = ""
                });

            // Assert
            Assert.Null(result);

            userRepositoryMock.Verify(
                x => x.Update(It.IsAny<User>()),
                Times.Never);

            userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Never);
        }
        [Fact]
        public void UpdateUser_ShouldHashNewPassword_WhenPasswordIsProvided()
        {
            // Arrange
            var userRepositoryMock = new Mock<IUserRepository>();
            var mapperMock = new Mock<IMapper>();
            var configurationMock = new Mock<IConfiguration>();

            var errorMessageService =
                new ErrorMessageService(
                    "errorMessages.tr.json",
                    "errorMessages.en.json",
                    new HttpContextAccessor());

            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com",
                Password = "old-password"
            };

            var updatedUserDto = new UserDto
            {
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com",
                Password = "YeniSifre123"
            };

            var resultDto = new UserDto
            {
                Id = userId,
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com"
            };

            userRepositoryMock
                .Setup(x => x.GetById(userId))
                .Returns(user);

            mapperMock
                .Setup(x => x.Map<UserDto>(user))
                .Returns(resultDto);

            var oldPassword = user.Password;

            var userService = new UserService(
                userRepositoryMock.Object,
                mapperMock.Object,
                configurationMock.Object,
                errorMessageService);

            // Act
            var result = userService.UpdateUser(
                userId,
                updatedUserDto);

            // Assert
            Assert.NotNull(result);

            Assert.NotEqual(oldPassword, user.Password);

            var passwordHasher = new PasswordHasher<User>();

            var verificationResult =
                passwordHasher.VerifyHashedPassword(
                    user,
                    user.Password,
                    "YeniSifre123");

            Assert.Equal(
                PasswordVerificationResult.Success,
                verificationResult);

            userRepositoryMock.Verify(
                x => x.Update(user),
                Times.Once);

            userRepositoryMock.Verify(
                x => x.SaveChanges(),
                Times.Once);
        }
    }
}