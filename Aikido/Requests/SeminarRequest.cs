using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class SeminarRequest
    {
        public IFormFile SeminarDataJson { get; set; }

        public async Task<SeminarDto> Parse()
        {
            using var reader = new StreamReader(SeminarDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var seminarData = JsonSerializer.Deserialize<SeminarDto>(jsonString, options);

            if (seminarData == null)
                throw new Exception("Не удалось десериализовать JSON.");

            return seminarData;
        }
    }
}
