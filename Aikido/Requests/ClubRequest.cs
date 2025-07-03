using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class ClubRequest
    {
        public IFormFile ClubDataJson { get; set; }

        public async Task<ClubDto> Parse()
        {
            using var reader = new StreamReader(ClubDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var userData = JsonSerializer.Deserialize<ClubDto>(jsonString, options);

            if (userData == null)
                throw new Exception("Не удалось десериализовать JSON.");

            return userData;
        }
    }
}
