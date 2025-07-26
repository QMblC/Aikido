using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class SeminarMembersListRequest
    {
        public IFormFile SeminarMembersDataJson { get; set; }

        public async Task<List<SeminarMemberDto>> Parse()
        {
            using var reader = new StreamReader(SeminarMembersDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var membersData = JsonSerializer.Deserialize<List<SeminarMemberDto>>(jsonString, options);

            if (membersData == null)
                throw new Exception("Не удалось десериализовать JSON список участников.");

            return membersData;
        }
    }
}
