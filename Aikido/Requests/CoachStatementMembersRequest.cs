using Aikido.Dto.Seminars;
using System.Text.Json;

namespace Aikido.Requests
{
    public class CoachStatementMembersRequest
    {
        public IFormFile MembersListJson { get; set; }

        public async Task<List<SeminarMemberDto>> Parse()
        {
            using var reader = new StreamReader(MembersListJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var members = JsonSerializer.Deserialize<List<SeminarMemberDto>>(jsonString, options);

            if (members == null || !members.Any())
                throw new Exception("Не удалось десериализовать JSON или список пуст.");

            return members;
        }
    }
}
