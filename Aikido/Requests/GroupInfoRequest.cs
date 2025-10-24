using Aikido.Dto.Groups;
using System.Text.Json;

namespace Aikido.Requests
{
    public class GroupInfoRequest
    {
        public IFormFile GroupInfoJson { get; set; } = default!;

        public async Task<GroupInfoDto> ParseGroupInfoAsync()
        {
            if (GroupInfoJson == null)
                throw new Exception("Файл с данными группы не предоставлен.");

            using var reader = new StreamReader(GroupInfoJson.OpenReadStream());
            var json = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var dto = JsonSerializer.Deserialize<GroupInfoDto>(json, options);
            if (dto == null)
                throw new Exception("Не удалось десериализовать JSON.");

            return dto;
        }
    }

}
