using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class JsonRequest
    {
        public IFormFile DataJson { get; set; }

        public T Parse<T>() where T : DtoBase
        {
            using var reader = new StreamReader(DataJson.OpenReadStream(), System.Text.Encoding.UTF8);
            var jsonString = reader.ReadToEnd();

            return JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Не удалось десериализовать JSON.");
        }
    }
}
