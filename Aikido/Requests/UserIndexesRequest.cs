using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class UserIndexesRequest
    {
        public IFormFile IndexesJson { get; set; }

        public async Task<UserIndexesDto> Parse()
        {
            using var reader = new StreamReader(IndexesJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var userIndexes = JsonSerializer.Deserialize<UserIndexesDto>(jsonString, options);

            if (userIndexes == null)
                throw new Exception("Не удалось десериализовать JSON.");

            return userIndexes;
        }
    }
}
