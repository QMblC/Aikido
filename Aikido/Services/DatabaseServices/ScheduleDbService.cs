using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class ScheduleDbService
    {
        private readonly AppDbContext _context;

        public ScheduleDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ScheduleEntity> GetScheduleById(long id)
        {
            var schedule = await _context.Schedule.FindAsync(id);
            if (schedule == null)
                throw new EntityNotFoundException($"Расписание с Id = {id} не найдено");
            return schedule;
        }

        public async Task<List<ScheduleEntity>> GetSchedulesByGroup(long groupId)
        {
            return await _context.Schedule
                .Where(s => s.GroupId == groupId && s.ClosedAt == null)
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<ScheduleEntity>> GetAllSchedules()
        {
            return await _context.Schedule
                .Where (s => s.ClosedAt == null)
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<bool> ScheduleExists(long id)
        {
            return await _context.Schedule.AnyAsync(s => s.Id == id);
        }

        public async Task CloseSchedule(long id)
        {
            var schedule = await GetScheduleById(id);
            schedule.ClosedAt = DateTime.UtcNow;
            _context.Schedule.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task CloseGroupSchedules(long groupId)
        {
            var schedules = await GetSchedulesByGroup(groupId);
            foreach (var schedule in schedules)
            {
                schedule.ClosedAt = DateTime.UtcNow;
            }
            _context.Schedule.UpdateRange(schedules);
            await _context.SaveChangesAsync();

        }

        public async Task RecoverSchedule(long id)
        {
            var schedule = await GetScheduleById(id);
            schedule.ClosedAt = null;
            _context.Schedule.Update(schedule);
            await _context.SaveChangesAsync();
        }
    }
}
