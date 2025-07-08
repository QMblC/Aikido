using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class EventRequest
    {
        public IFormFile EventDataJson { get; set; }

        public async Task<EventDto> Parse()
        {
            using var reader = new StreamReader(EventDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var eventData = JsonSerializer.Deserialize<EventDto>(jsonString, options);

            if (eventData == null)
                throw new Exception("Не удалось десериализовать JSON.");

            return eventData;
        }
    }
}
