using Aikido.Dto.Users;
using Microsoft.AspNetCore.Http;
using System.Text.Json;


namespace Aikido.Requests
{
    public class UserListRequest
    {
        public IFormFile UserListJson { get; set; }

        public async Task<List<UserDto>> Parse()
        {
            using var reader = new StreamReader(UserListJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var users = JsonSerializer.Deserialize<List<UserDto>>(jsonString, options);

            if (users == null || !users.Any())
                throw new Exception("Не удалось десериализовать JSON или список пуст.");

            return users;
        }
    }
}
