using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class AttendanceService : DbService
    {
        public AttendanceService(AppDbContext context) : base(context)
        {

        }

        public async Task AddAttendance(AttendanceDto attendance)
        {
            var attendanceEntity = new AttendanceEntity();
            await attendanceEntity.UpdateFromJson(attendance);

            await context.Attendances.AddAsync(attendanceEntity);

            await SaveChangesAsync();
        }

        public async Task RemoveAttendance(AttendanceDto attendandce)
        {
            var attendanceEntity = await context.Clubs.FindAsync(attendandce.Id);
            if (attendanceEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {attendandce.Id} не найден.");

            context.Remove(attendanceEntity);

            await SaveChangesAsync();
        }

        public async Task<List<AttendanceEntity>> GetUserMonthlyAttendance(long userId, long groupId, DateTime month)
        {
            return await context.Attendances
                .Where(a => a.UserId == userId)
                .Where(a => a.GroupId == groupId)
                .Where(a => a.VisitDate.Value.Year == month.Year)
                .Where(a => a.VisitDate.Value.Month == month.Month)
                .ToListAsync();
        }
    }
}