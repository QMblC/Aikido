using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class AttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AttendanceEntity> GetAttendanceById(long id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null)
                throw new EntityNotFoundException($"Посещаемость с Id = {id} не найдена");
            return attendance;
        }

        public async Task<List<AttendanceEntity>> GetAttendanceByUser(long userId)
        {
            return await _context.Attendances
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<List<AttendanceEntity>> GetAttendanceByEvent(long eventId)
        {
            return await _context.Attendances
                .Where(a => a.EventId == eventId)
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<long> CreateAttendance(AttendanceDto attendanceData)
        {
            var entity = new AttendanceEntity(attendanceData);
            _context.Attendances.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task UpdateAttendance(long id, AttendanceDto attendanceData)
        {
            var attendance = await GetAttendanceById(id);
            attendance.UpdateFromJson(attendanceData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAttendance(long id)
        {
            var attendance = await GetAttendanceById(id);
            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AttendanceExists(long id)
        {
            return await _context.Attendances.AnyAsync(a => a.Id == id);
        }
    }
}
