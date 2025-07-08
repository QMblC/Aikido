using Aikido.Dto;
using System.Text.Json;

namespace Aikido.Requests
{
    public class GroupRequest
    {
        public IFormFile GroupDataJson { get; set; }
        public IFormFile? ScheduleDataJson { get; set; }

        public async Task<GroupDto> ParseGroupAsync()
        {
            if (GroupDataJson == null)
                throw new Exception("Файл данных группы не предоставлен.");

            using var reader = new StreamReader(GroupDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var groupData = JsonSerializer.Deserialize<GroupDto>(jsonString, options);
            if (groupData == null)
                throw new Exception("Не удалось десериализовать данные группы.");

            return groupData;
        }

        public async Task<List<ScheduleDto>> ParseScheduleAsync()
        {
            if (ScheduleDataJson == null)
                return new List<ScheduleDto>(); // Пустой список — это ок

            using var reader = new StreamReader(ScheduleDataJson.OpenReadStream());
            var jsonString = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var scheduleList = JsonSerializer.Deserialize<List<ScheduleDto>>(jsonString, options);
            if (scheduleList == null)
                throw new Exception("Не удалось десериализовать расписание.");

            return scheduleList;
        }
    }

}
