using Aikido.Data;
using Aikido.Dto.Attendance;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class AttendanceDbService
    {
        private readonly AppDbContext _context;

        public AttendanceDbService(AppDbContext context)
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

        public async Task<List<AttendanceEntity>> GetAttendancesByGroup(long groupId, DateTime date)
        {
            return await _context.Attendances
                .AsQueryable()
                .Where(a => a.UserMembershipId == groupId
                && a.Date.Year == date.Year 
                && a.Date.Month == date.Month)
                .Include(a => a.UserMembership)
                .ToListAsync();
        }

        public async Task<List<AttendanceEntity>> GetAttendanceByUser(long userId)
        {
            return await _context.Attendances
                .Where(a => a.UserMembership.UserId == userId)
                .Include(a => a.UserMembership)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<long> CreateAttendance(UserMembershipEntity userMembership, DateTime date)
        {
            var entity = new AttendanceEntity(userMembership, date);
            _context.Attendances.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task DeleteAttendance(long id)
        {
            var attendance = await GetAttendanceById(id);
            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAttendances(List<long> ids)
        {
            var attendanceEntities = await _context.Attendances
                .AsQueryable()
                .Where(a => ids.Any(id => a.Id == id))
                .ToListAsync();

            _context.RemoveRange(attendanceEntities);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AttendanceExists(long id)
        {
            return await _context.Attendances.AnyAsync(a => a.Id == id);
        }

        public async Task<bool> AttendanceExists(long userId, DateTime date)
        {
            return await _context.Attendances.AnyAsync(a => a.UserMembership.UserId == userId && a.Date == date);
        }
    }
}
