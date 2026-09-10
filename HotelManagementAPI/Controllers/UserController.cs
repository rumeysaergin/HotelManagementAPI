using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Services;
using HotelManagementAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ErrorMessageService _errorMessageService;

        public UserController(
            IUserService userService,
            ErrorMessageService errorMessageService)
        {
            _userService = userService;
            _errorMessageService = errorMessageService;
        }

        [HttpPost("register")]
        public IActionResult Register(UserDto userDto)
        {
            try
            {
                var result = _userService.Register(userDto);

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest loginRequest)
        {
            try
            {
                var token = _userService.Login(
                    loginRequest.Email,
                    loginRequest.Password);

                if (token == null)
                {
                    return Unauthorized(
                        "LOGIN_RESULT: Kullanıcı bulunamadı.");
                }

                return Ok(new
                {
                    message = "Giriş başarılı.",
                    token = token
                });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var user = _userService.GetProfile(
                Guid.Parse(userId));

            if (user == null)
            {
                var error =
                    _errorMessageService.Get("USER_NOT_FOUND");

                return NotFound(
                    $"{error.Code}: {error.Message}");
            }

            return Ok(user);
        }

        [HttpPut("update")]
        [Authorize]
        public IActionResult UpdateUser(
            UserDto updatedUserDto)
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return Unauthorized();

            var user = _userService.UpdateUser(
                Guid.Parse(userId),
                updatedUserDto);

            if (user == null)
            {
                var error =
                    _errorMessageService.Get("USER_NOT_FOUND");

                return NotFound(
                    $"{error.Code}: {error.Message}");
            }

            return Ok(new
            {
                message =
                    "Kullanıcı bilgileri başarıyla güncellendi.",
                user = user
            });
        }
    }
}