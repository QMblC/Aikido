using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Entities;
using Aikido.Entities.Filters;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.StatisticService
{
    public class StatisticDbService : IStatisticDbService
    {
        private readonly AppDbContext _context;

        public StatisticDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetYearlyAttendances(int year, StatAttendanceFilter filter)
        {
            var query = _context.Attendances
                .Where(a => a.Date.Year == year);

            query = FilterStat(query, filter);

            var count = await query.CountAsync();

            return count;
        }

        public async Task<Dictionary<int, int>> GetYearlyAttendances(int firstYear, int lastYear, StatAttendanceFilter filter)
        {
            var attendances = new Dictionary<int, int>();

            for(var year = firstYear; year <= lastYear; year++)
            {
                var count = await GetYearlyAttendances(year, filter);

                attendances.Add(year, count);
            }

            return attendances;
        }

        public async Task<int> GetYearlyTrainings(int year, StatAttendanceFilter filter)
        {
            var regularTrainingQuery = _context.Schedule
                .Include(s => s.Group)
                    .ThenInclude(g => g.UserMemberships)
                .AsQueryable();
            regularTrainingQuery = FilterStat(regularTrainingQuery, filter);

            var schedules = await regularTrainingQuery.ToListAsync();

            var totalTrainings = 0;

            foreach (var schedule in schedules)
            {
                totalTrainings += CountDayOfWeekInYear(year, schedule.DayOfWeek) * schedule.Group.UserMemberships.Count(um => um.RoleInGroup == Role.User);
            }

            var exclusionQuery = _context.ExclusionDates.AsQueryable();
            exclusionQuery = FilterStat(exclusionQuery, filter);

            var exclusionDates = await exclusionQuery
                .Where(e => e.Date.Year == year)
                .Include(s => s.Group)
                    .ThenInclude(g => g.UserMemberships)
                .ToListAsync();

            var minorCount = exclusionDates.Where(e => e.Type == ExclusiveDateType.Minor)
                .Select(e => e.Group.UserMemberships.Count(um => um.RoleInGroup == Role.User))
                .Sum();
            var extraCount = exclusionDates.Where(e => e.Type == ExclusiveDateType.Extra)
                .Select(e => e.Group.UserMemberships.Count(um => um.RoleInGroup == Role.User))
                .Sum();

            return totalTrainings - minorCount + extraCount;
        }

        public async Task<Dictionary<int, int>> GetYearlyTrainings(int firstYear, int lastYear, StatAttendanceFilter filter)
        {
            var trainings = new Dictionary<int, int>();

            for (var year = firstYear; year <= lastYear; year++)
            {
                var count = await GetYearlyTrainings(year, filter);

                trainings.Add(year, count);
            }

            return trainings;
        }

        private int CountDayOfWeekInYear(int year, DayOfWeek dayOfWeek)
        {
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31);

            if (DateTime.UtcNow.Year == year)
            {
                var yesterday = DateTime.UtcNow.Date.AddDays(-1);
                end = yesterday < start ? start : yesterday;
            }

            var totalDays = (end - start).Days + 1;

            var fullWeeks = totalDays / 7;
            var remainingDays = totalDays % 7;

            var count = fullWeeks;

            for (int i = 0; i < remainingDays; i++)
            {
                if (start.AddDays(fullWeeks * 7 + i).DayOfWeek == dayOfWeek)
                    count++;
            }

            return count;
        }

        public async Task<Dictionary<DateTime, int>> GetYearlyAttendancesByMonthes(int year, StatAttendanceFilter filter)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetMonthlyAttendances(int year, int month, StatAttendanceFilter filter)
        {
            throw new NotImplementedException();
        }

        public async Task<double> GetAverageMonthlyAttendancePercent(int year, int month, StatAttendanceFilter filter)
        {
            throw new NotImplementedException();
        }

        private static IQueryable<AttendanceEntity> FilterStat(IQueryable<AttendanceEntity> query, StatAttendanceFilter filter)
        {
            if (filter.ClubIds != null)
            {
                query = query.Where(a => filter.ClubIds.Any(clubId => clubId == a.UserMembership.ClubId));
            }
            if (filter.GroupIds != null)
            {
                query = query.Where(a => filter.GroupIds.Any(groupId => groupId == a.UserMembership.GroupId));
            }
            if (filter.Grades != null)
            {
                var gradeEnums = filter.Grades.Select(EnumParser.ConvertStringToEnum<Grade>).ToList();
                query = query.Where(a => gradeEnums.Contains(a.UserMembership.User.Grade));
            }
            if (filter.Cities != null)
            {
                var lowerCities = filter.Cities.Select(c => c.ToLower()).ToList();
                query = query.Where(a => a.UserMembership.User.City != null && lowerCities.Contains(a.UserMembership.User.City.ToLower()));
            }
            if (filter.CoachId != null)
            {
                query = query.Where(a => a.UserMembership.Group.UserMemberships.Any(u => u.RoleInGroup == Role.Coach && u.Id == filter.CoachId));
            }

            return query;
        }

        private static IQueryable<ScheduleEntity> FilterStat(IQueryable<ScheduleEntity> query, StatAttendanceFilter filter)
        {//Возможно стоит реализовать города и пояса, но как?
            if (filter.ClubIds != null)
            {
                query = query.Where(s => filter.ClubIds.Any(clubId => clubId == s.Group.ClubId));
            }
            if (filter.GroupIds != null)
            {
                query = query.Where(s => filter.GroupIds.Any(groupId => groupId == s.GroupId));
            }
            if (filter.CoachId != null)
            {
                query = query.Where(s => s.Group.UserMemberships.Any(u => u.RoleInGroup == Role.Coach && u.Id == filter.CoachId));
            }

            return query;
        }

        private static IQueryable<ExclusionDateEntity> FilterStat(IQueryable<ExclusionDateEntity> query, StatAttendanceFilter filter)
        {
            if (filter.ClubIds != null)
            {
                query = query.Where(e => filter.ClubIds.Any(clubId => clubId == e.Group.ClubId));
            }
            if (filter.GroupIds != null)
            {
                query = query.Where(e => filter.GroupIds.Any(groupId => groupId == e.GroupId));
            }
            if (filter.CoachId != null)
            {
                query = query.Where(e => e.Group.UserMemberships.Any(u => u.RoleInGroup == Role.Coach && u.Id == filter.CoachId));
            }

            return query;
        }
    }
}
