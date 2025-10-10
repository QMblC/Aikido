using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class ScheduleService
    {
        private readonly AppDbContext _context;

        public ScheduleService(AppDbContext context)
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
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<List<ScheduleEntity>> GetAllSchedules()
        {
            return await _context.Schedule
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<bool> ScheduleExists(long id)
        {
            return await _context.Schedule.AnyAsync(s => s.Id == id);
        }
    }
}
