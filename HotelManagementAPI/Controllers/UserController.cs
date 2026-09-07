using AutoMapper;
using HotelManagementAPI.Data;
using HotelManagementAPI.Entities;
using HotelManagementAPI.Models;
using HotelManagementAPI.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly HotelManagementDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserController(
            HotelManagementDbContext context,
            IConfiguration configuration,
            IMapper mapper)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(x =>
                x.Email == user.Email &&
                !x.IsDeleted))
            {
                return BadRequest(
                    "Bu e-posta adresi zaten kayıtlı.");
            }

            user.Id = Guid.NewGuid();
            user.IsDeleted = false;

            user.Password = _passwordHasher.HashPassword(
                user,
                user.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(new
            {
                message = "Kullanıcı başarıyla oluşturuldu.",
                user = userDto
            });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Email == loginRequest.Email &&
                    !x.IsDeleted);

            if (user == null)
            {
                return Unauthorized(
                    "E-posta veya şifre hatalı.");
            }

            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.Password,
                    loginRequest.Password);

            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                return Unauthorized(
                    "E-posta veya şifre hatalı.");
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
                    $"{user.FirstName} {user.LastName}")
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
                signingCredentials: credentials);

            var tokenString =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);

            return Ok(new
            {
                message = "Giriş başarılı.",
                token = tokenString
            });
        }

        [HttpGet("profile")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult GetProfile()
        {
            var userId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Id == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (user == null)
            {
                return NotFound(
                    "Kullanıcı bulunamadı.");
            }

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(userDto);
        }

        [HttpPut("update")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult UpdateUser(User updatedUser)
        {
            var userId =
                User.FindFirst(
                    ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            var user = _context.Users
                .FirstOrDefault(x =>
                    x.Id == Guid.Parse(userId) &&
                    !x.IsDeleted);

            if (user == null)
            {
                return NotFound(
                    "Kullanıcı bulunamadı.");
            }

            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;

            _context.SaveChanges();

            var userDto = _mapper.Map<UserDto>(user);

            return Ok(new
            {
                message =
                    "Kullanıcı bilgileri başarıyla güncellendi.",
                user = userDto
            });
        }
    }
}