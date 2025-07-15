using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class SeminarMemberRequest
    {
        public IFormFile SeminarMemberDataJson { get; set; }

        public async Task<SeminarMemberDto> Parse()
        {
            using var reader = new StreamReader(SeminarMemberDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var memberData = JsonSerializer.Deserialize<SeminarMemberDto>(jsonString, options);

            if (memberData == null)
                throw new Exception("Не удалось десериализовать JSON участника.");

            return memberData;
        }
    }
}
