using System.Text.Json;

namespace Aikido.Requests
{

    public static class JsonHelper
    {
        public static async Task<T> DeserializeJsonFormFileAsync<T>(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream(), System.Text.Encoding.UTF8);
            var jsonString = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Не удалось десериализовать JSON.");
        }
    }

}
