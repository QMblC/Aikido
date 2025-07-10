using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class AttendanceRequest
    {
        public IFormFile AttendanceDataJson { get; set; }

        public async Task<AttendanceDto> Parse()
        {
            using var reader = new StreamReader(AttendanceDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var attendanceData = JsonSerializer.Deserialize<AttendanceDto>(jsonString, options);

            if (attendanceData == null)
                throw new Exception("Не удалось десериализовать JSON.");

            return attendanceData;
        }
    }
}
