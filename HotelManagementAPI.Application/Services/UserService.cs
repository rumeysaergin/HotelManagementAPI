using AutoMapper;
using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelManagementAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly ErrorMessageService _errorMessageService;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            IConfiguration configuration,
            ErrorMessageService errorMessageService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
            _errorMessageService = errorMessageService;
            _passwordHasher = new PasswordHasher<User>();
        }

        public object Register(UserDto userDto)
        {
            if (_userRepository.EmailExists(userDto.Email))
            {
                var error = _errorMessageService.Get("USER_EMAIL_EXISTS");

                throw new InvalidOperationException(
                    $"{error.Code}: {error.Message}");
            }

            var user = _mapper.Map<User>(userDto);

            user.Id = Guid.NewGuid();
            user.Balance = 10000;
            user.IsDeleted = false;

            user.Password = _passwordHasher.HashPassword(
                user,
                userDto.Password);

            _userRepository.Add(user);
            _userRepository.SaveChanges();

            return new
            {
                message = "Kullanıcı başarıyla kayıt oldu.",
                user = _mapper.Map<UserDto>(user)
            };
        }

        public string? Login(string email, string password)
        {
            var user = _userRepository.GetByEmail(email);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "DEBUG: Kullanıcı GetByEmail ile bulunamadı.");
            }

            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.Password,
                    password);

            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                throw new InvalidOperationException(
                    "DEBUG: Şifre doğrulaması başarısız.");
            }

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Name,
                    user.FirstName + " " + user.LastName)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(
                        _configuration["Jwt:ExpireMinutes"]!)),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public UserDto? GetProfile(Guid userId)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
                return null;

            return _mapper.Map<UserDto>(user);
        }

        public UserDto? UpdateUser(
            Guid userId,
            UserDto updatedUserDto)
        {
            var user = _userRepository.GetById(userId);

            if (user == null)
                return null;

            user.FirstName = updatedUserDto.FirstName;
            user.LastName = updatedUserDto.LastName;
            user.Email = updatedUserDto.Email;

            if (!string.IsNullOrWhiteSpace(updatedUserDto.Password))
            {
                user.Password = _passwordHasher.HashPassword(
                    user,
                    updatedUserDto.Password);
            }

            _userRepository.Update(user);
            _userRepository.SaveChanges();

            return _mapper.Map<UserDto>(user);
        }
    }
}