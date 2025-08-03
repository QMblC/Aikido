using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore; // ← обязательно



namespace Aikido.Services
{
    public class ScheduleService : DbService
    {

        public ScheduleService(AppDbContext context) : base(context) { }

        public async Task CreateGroupScheduleDeleteExcess(long groupId, Dictionary<string, string> schedule)
        {
            var existingSchedules = await context.Schedule
                .Where(s => s.GroupId == groupId)
                .ToListAsync();

            context.Schedule.RemoveRange(existingSchedules);

            foreach (var day in schedule)
            {
                var times = day.Value.Split('-');
                if (times.Length != 2 ||
                    !TimeSpan.TryParse(times[0], out var startTime) ||
                    !TimeSpan.TryParse(times[1], out var endTime))
                {
                    throw new FormatException($"Неверный формат времени: {day.Value}");
                }

                if (!TryParseRussianDayOfWeek(day.Key, out var dayOfWeek))
                {
                    throw new ArgumentException($"Неверный день недели: {day.Key}");
                }

                context.Schedule.Add(new ScheduleEntity
                {
                    GroupId = groupId,
                    DayOfWeek = dayOfWeek,
                    StartTime = startTime,
                    EndTime = endTime
                });
            }

            await SaveChangesAsync();
        }

        private bool TryParseRussianDayOfWeek(string input, out DayOfWeek result)
        {
            result = input switch
            {
                "Пн" => DayOfWeek.Monday,
                "Вт" => DayOfWeek.Tuesday,
                "Ср" => DayOfWeek.Wednesday,
                "Чт" => DayOfWeek.Thursday,
                "Пт" => DayOfWeek.Friday,
                "Сб" => DayOfWeek.Saturday,
                "Вс" => DayOfWeek.Sunday,
                _ => (DayOfWeek)(-1)
            };

            return result != (DayOfWeek)(-1);
        }



        public async Task<List<ScheduleEntity>> GetGroupSchedule(long groupId)
        {
            return await context.Schedule
                .Where(s => s.GroupId == groupId)
                .ToListAsync();
        }

        public async Task CreateExclusionDateDeleteExcess(long groupId, List<ExclusionDateDto> dates)
        {
            var existing = await context.ExclusionDates
                .Where(e => e.GroupId == groupId)
                .ToListAsync();

            context.ExclusionDates.RemoveRange(existing);

            foreach (var dto in dates)
            {

                var utcDate = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc);

                var entity = new ExclusionDateEntity
                {
                    GroupId = groupId,
                    Date = utcDate,
                    Status = EnumParser.ConvertStringToEnum<ExclusiveDateType>(dto.Status)
                };

                context.ExclusionDates.Add(entity);
            }

            await SaveChangesAsync();
        }

        public async Task<List<ExclusionDateEntity>> GetGroupExclusionDates(long groupId, DateTime month)
        {
            return await context.ExclusionDates
                .Where(d => d.GroupId == groupId)
                .Where(d => d.Date.Month == month.Month)
                .Where(d => d.Date.Year == month.Year)
                .ToListAsync();
        }
    }

}
