using Aikido.Data;
using Aikido.Entities;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore; // ← обязательно



namespace Aikido.Services
{
    public class ScheduleService
    {
        private readonly AppDbContext context;

        public ScheduleService(AppDbContext context)
        {
            this.context = context;
        }

        private async Task SaveDb()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обработке расписания: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task CreateGroupScheduleDeleteExcess(long groupId, List<DayOfWeek> weekDays)
        {
            var existingSchedules = await context.Schedule
                .Where(s => s.GroupId == groupId)
                .ToListAsync();

            context.Schedule.RemoveRange(existingSchedules);

            foreach (var day in weekDays)
            {
                context.Schedule.Add(new ScheduleEntity
                {
                    GroupId = groupId,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(18, 0, 0),
                    EndTime = new TimeSpan(19, 30, 0)  
                });
            }

            await SaveDb();
        }

        public async Task<List<ScheduleEntity>> GetGroupSchedule(long groupId)
        {
            return await context.Schedule
                .Where(s => s.GroupId == groupId)
                .ToListAsync();
        }
    }

}
