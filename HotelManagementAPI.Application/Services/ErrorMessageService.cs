using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace HotelManagementAPI.Application.Services
{
    public class ErrorMessageService
    {
        private readonly Dictionary<string, ErrorMessage> _trMessages;
        private readonly Dictionary<string, ErrorMessage> _enMessages;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ErrorMessageService(
            string trFilePath,
            string enFilePath,
            IHttpContextAccessor httpContextAccessor)
        {
            _trMessages = LoadMessages(trFilePath);
            _enMessages = LoadMessages(enFilePath);
            _httpContextAccessor = httpContextAccessor;
        }

        public ErrorMessage Get(string key)
        {
            var language = _httpContextAccessor
                .HttpContext?
                .Request
                .Headers["Accept-Language"]
                .FirstOrDefault();

            var messages = language?.ToLower().StartsWith("en") == true
                ? _enMessages
                : _trMessages;

            if (messages.TryGetValue(key, out var message))
            {
                return message;
            }

            return new ErrorMessage
            {
                Code = "UNKNOWN",
                Message = language?.ToLower().StartsWith("en") == true
                    ? "An unknown error occurred."
                    : "Bilinmeyen bir hata oluştu."
            };
        }

        private Dictionary<string, ErrorMessage> LoadMessages(
            string filePath)
        {
            var json = File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<
                Dictionary<string, ErrorMessage>>(json)
                ?? new Dictionary<string, ErrorMessage>();
        }
    }

    public class ErrorMessage
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}