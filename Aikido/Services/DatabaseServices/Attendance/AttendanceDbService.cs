using Aikido.Data;
using Aikido.Dto.Attendance;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.Attendance
{
    public class AttendanceDbService : IAttendanceDbService
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
                .Where(a => a.UserMembership.GroupId == groupId
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

        public async Task<long> CreateAttendance(long userMembershipId, DateTime date)
        {
            var entity = new AttendanceEntity(userMembershipId, date);
            _context.Attendances.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task CreateAttendances(Dictionary<long, List<DateTime>> data)
        {
            var attendanceEntities = new List<AttendanceEntity>();

            foreach(var pair in data)
            {
                foreach(var attendance in pair.Value)
                {
                    var attendanceEntity = new AttendanceEntity(pair.Key, attendance);
                    attendanceEntities.Add(attendanceEntity);
                }
            }
            await _context.Attendances.AddRangeAsync(attendanceEntities);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAttendances(List<long> idsToDelete)
        {
            var attendancesToDelete = _context.Attendances
                .Where(a => idsToDelete.Contains(a.Id));

            _context.Attendances.RemoveRange(attendancesToDelete);
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

        public async Task<bool> AttendanceExists(long userId, DateTime date)
        {
            return await _context.Attendances.AnyAsync(a => a.UserMembership.UserId == userId && a.Date == date);
        }
    }
}
