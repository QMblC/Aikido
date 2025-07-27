using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class UserRequest
    {
        public IFormFile UserDataJson { get; set; }

        public async Task<UserDto> Parse()
        {
            using var reader = new StreamReader(UserDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var userData = JsonSerializer.Deserialize<UserDto>(jsonString, options);

            if (userData == null)
                throw new Exception("Не удалось десериализовать JSON.");

            return userData;
        }
    }
}