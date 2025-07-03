using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class GroupRequest
    {
        public IFormFile GroupDataJson { get; set; }

        public async Task<GroupDto> Parse()
        {
            using var reader = new StreamReader(GroupDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var groupData = JsonSerializer.Deserialize<GroupDto>(jsonString, options);

            if (groupData == null)
                throw new Exception("Не удалось десериализовать JSON.");

            return groupData;
        }
    }
}
