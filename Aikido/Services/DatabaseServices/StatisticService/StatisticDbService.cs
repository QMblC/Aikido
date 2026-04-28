using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Entities;
using Aikido.Entities.Filters;
using Aikido.Entities.Users;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Aikido.Services.DatabaseServices.StatisticService
{
    public class StatisticDbService : IStatisticDbService
    {
        private readonly AppDbContext _context;

        public StatisticDbService(AppDbContext context)
        {
            _context = context;
        }

        #region Attendance

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
            var start = DateTime.SpecifyKind(new DateTime(year, 1, 1), DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(new DateTime(year, 12, 31), DateTimeKind.Utc);

            if (year == DateTime.UtcNow.Year)
            {
                end = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-1), DateTimeKind.Utc);
            }

            var scheduleQuery = _context.Schedule
                .Include(s => s.Group)
                    .ThenInclude(g => g.UserMemberships)
                .AsQueryable();

            scheduleQuery = FilterStat(scheduleQuery, filter);

            scheduleQuery = scheduleQuery.Where(s =>
                s.CreatedAt <= end &&
                (s.ClosedAt == null || s.ClosedAt >= start));

            var schedules = await scheduleQuery.ToListAsync();

            var exclusionDates = await _context.ExclusionDates
                .Include(e => e.Group)
                    .ThenInclude(g => g.UserMemberships)
                .Where(e => e.Date >= start && e.Date <= end)
                .ToListAsync();

            var exclusionLookup = exclusionDates.ToLookup(e => (e.GroupId, e.Date.Date));

            int totalTrainings = 0;

            foreach (var schedule in schedules)
            {
                var scheduleStart = schedule.CreatedAt > start
                    ? schedule.CreatedAt.Date
                    : start;

                var scheduleEnd = schedule.ClosedAt.HasValue && schedule.ClosedAt.Value < end
                    ? schedule.ClosedAt.Value.Date
                    : end;

                var firstTrainingDate = GetFirstDayOfWeek(scheduleStart, schedule.DayOfWeek);

                for (var date = firstTrainingDate; date <= scheduleEnd; date = date.AddDays(7))
                {
                    var exclusions = exclusionLookup[(schedule.GroupId.Value, date)];

                    if (exclusions.Any(e => e.Type == ExclusiveDateType.Minor))
                        continue;

                    var members = schedule.Group.UserMemberships
                        .Where(m =>
                            m.RoleInGroup == Role.User &&
                            m.CreateAt.Date <= date &&
                            (m.ClosedAt == null || m.ClosedAt.Value.Date >= date))
                        .Count();

                    totalTrainings += members;
                }
            }

            var extraTrainings = exclusionDates
                .Where(e => e.Type == ExclusiveDateType.Extra);

            foreach (var extra in extraTrainings)
            {
                var members = extra.Group.UserMemberships
                    .Where(m =>
                        m.RoleInGroup == Role.User &&
                        m.CreateAt.Date <= extra.Date &&
                        (m.ClosedAt == null || m.ClosedAt.Value.Date >= extra.Date))
                    .Count();

                totalTrainings += members;
            }

            return totalTrainings;
        }

        private static DateTime GetFirstDayOfWeek(DateTime start, DayOfWeek dayOfWeek)
        {
            int diff = ((int)dayOfWeek - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(diff);
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


        public async Task<Dictionary<DateTime, int>> GetYearlyAttendancesByMonthes(int year, StatAttendanceFilter filter)
        {
            var start = DateTime.SpecifyKind(new DateTime(year - 1, 12, 1), DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(new DateTime(year, 12, 31), DateTimeKind.Utc);

            if (year == DateTime.UtcNow.Year)
            {
                end = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddDays(-1), DateTimeKind.Utc);
            }

            var query = _context.Attendances
                .Where(a => a.Date >= start && a.Date <= end);

            query = FilterStat(query, filter);

            var data = await query
                .GroupBy(a => new { a.Date.Year, a.Date.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync();

            var result = new Dictionary<DateTime, int>();

            for (int i = 0; i < 13; i++)
            {
                var month = start.AddMonths(i);
                var monthStart = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);

                var count = data
                    .FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Count ?? 0;

                result[monthStart] = count;
            }

            return result;
        }

        #endregion

        #region PupilGrow

        public async Task<Dictionary<DateTime, int>> GetMonthlyPupilAmount(int year, StatAttendanceFilter filter)
        {
            var start = new DateTime(year - 1, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            if (year == DateTime.UtcNow.Year)
            {
                end = DateTime.UtcNow.Date.AddDays(-1);
            }

            var memberships = _context.UserMemberships
                .Where(x => x.RoleInGroup == Role.User);

            memberships = FilterStat(memberships, filter);

            var data = await memberships
                .Select(x => new
                {
                    x.UserId,
                    x.CreateAt,
                    x.ClosedAt
                })
                .ToListAsync();

            var result = new Dictionary<DateTime, int>();

            for (int i = 0; i < 13; i++)
            {
                var monthStart = start.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var count = data
                    .Where(x =>
                        x.CreateAt <= monthEnd &&
                        (x.ClosedAt == null || x.ClosedAt >= monthStart))
                    .Select(x => x.UserId)
                    .Distinct()
                    .Count();

                result[monthStart] = count;
            }

            return result;
        }

        #endregion

        #region PupulRetention

        public async Task<Dictionary<DateTime, int>> GetPupilLeft(int year, StatAttendanceFilter filter)
        {
            var start = new DateTime(year - 1, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            if (year == DateTime.UtcNow.Year)
            {
                end = DateTime.UtcNow.Date.AddDays(-1);
            }

            var memberships = _context.UserMemberships
                .Where(x => x.RoleInGroup == Role.User && x.ClosedAt != null);

            memberships = FilterStat(memberships, filter);

            var closedMemberships = await memberships
                .Select(x => new
                {
                    x.UserId,
                    ClosedAt = x.ClosedAt!.Value
                })
                .ToListAsync();

            var result = new Dictionary<DateTime, int>();

            var monthsCount = year == DateTime.UtcNow.Year
                ? DateTime.UtcNow.Month + 1
                : 13;

            for (int i = 0; i < monthsCount; i++)
            {
                var monthStart = start.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var usersLeft = closedMemberships
                    .Where(x => x.ClosedAt >= monthStart && x.ClosedAt <= monthEnd)
                    .Select(x => x.UserId)
                    .Distinct()
                    .Where(userId =>
                    {
                        var lastClosed = closedMemberships
                            .Where(m => m.UserId == userId)
                            .Max(m => m.ClosedAt);

                        return lastClosed >= monthStart && lastClosed <= monthEnd;
                    })
                    .Count();

                result[monthStart] = usersLeft;
            }

            return result;
        }

        #endregion


        #region DanAmount

        public async Task<Dictionary<DateTime, int>> GetMonthlyDanAmount(int year, StatAttendanceFilter filter)
        {
            var start = new DateTime(year - 1, 12, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Utc);

            if (year == DateTime.UtcNow.Year)
                end = DateTime.UtcNow.Date.AddDays(-1);

            var monthsCount = year == DateTime.UtcNow.Year
                ? DateTime.UtcNow.Month + 1
                : 13;

            var seminarUsers = await _context.SeminarMembers
                .Where(x =>
                    x.Status == SeminarMemberStatus.Certified &&
                    x.CertificationGrade >= Grade.Dan1)
                .Select(x => new
                {
                    x.UserId,
                    Date = x.Seminar!.Date
                })
                .ToListAsync();

            var danUsersWithoutSeminar = await _context.Users
                .Where(u =>
                    u.Grade >= Grade.Dan1 &&
                    u.UserMemberships.Any(m => m.RoleInGroup == Role.User) &&
                    !u.Certifications.Any())
                .Select(u => new
                {
                    u.Id,
                    Date = u.RegistrationDate ?? u.CreatedAt
                })
                .ToListAsync();

            var memberships = await _context.UserMemberships
                .Where(x => x.RoleInGroup == Role.User)
                .Select(x => new
                {
                    x.UserId,
                    x.CreateAt,
                    x.ClosedAt,
                    x.User
                })
                .ToListAsync();

            var result = new Dictionary<DateTime, int>();

            for (int i = 0; i < monthsCount; i++)
            {
                var monthStart = start.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var seminarCount = seminarUsers
                    .Where(x => x.Date <= monthEnd)
                    .Select(x => x.UserId)
                    .Distinct()
                    .Where(userId =>
                        memberships.Any(m =>
                            m.UserId == userId &&
                            m.CreateAt <= monthEnd &&
                            (m.ClosedAt == null || m.ClosedAt >= monthStart)))
                    .Count();

                var noSeminarCount1 = danUsersWithoutSeminar
                    .Where(x => x.Date <= monthEnd)
                    .Select(x => x.Id)
                    .Distinct()
                    .Where(userId =>
                        memberships.Any(m =>
                            m.UserId == userId &&
                            m.CreateAt <= monthEnd &&
                            (m.ClosedAt == null || m.ClosedAt >= monthStart)))
                    .ToList();

                var noSeminarCount = danUsersWithoutSeminar
                    .Where(x => x.Date <= monthEnd)
                    .Select(x => x.Id)
                    .Distinct()
                    .Where(userId =>
                        memberships.Any(m =>
                            m.UserId == userId &&
                            m.User.RegistrationDate <= monthEnd &&
                            (m.User.ClosedAt == null || m.ClosedAt >= monthStart)))
                    .Count();

                result[monthStart] = seminarCount + noSeminarCount;
            }

            return result;
        }

        #endregion

        #region Seminars

        public async Task<int> GetActiveMembersCountAt(DateTime date)
        {
            var activeMembersCount = (await _context.UserMemberships
                .Where(um => um.CreateAt <= date && (um.ClosedAt > date || um.ClosedAt == null)
                    && um.RoleInGroup == Role.User)
                .GroupBy(um => um.UserId)
                .ToDictionaryAsync(u => u.Key, u => u.ToList()))
                .Count;
                

            return activeMembersCount;    
        }

        public async Task<Dictionary<long, int>> GetSeminarMemberCount(List<long> seminarIds)
        {
            return await _context.SeminarMembers
                .Where(s => seminarIds.Contains(s.SeminarId))
                .GroupBy(s => s.SeminarId)
                .Select(g => new
                {
                    SeminarId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.SeminarId, x => x.Count);
        }

        public int GetSeminarMemberCount(long seminarId)
        {
            return _context.SeminarMembers
                .Where(s => s.SeminarId == seminarId)
                .Count();
        }

        public async Task<Dictionary<long, int>> GetSeminarCertificatedMembers(List<long> seminarIds)
        {
            return await _context.SeminarMembers
                .Where(s => seminarIds.Contains(s.SeminarId) && s.CertificationGrade != null && s.CertificationGrade > 0)
                .GroupBy(s => s.SeminarId)
                .Select(g => new
                {
                    SeminarId = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.SeminarId, x => x.Count);
        }

        public async Task<Dictionary<long, decimal>> GetSeminarMoneyIncome(List<long> seminarIds)
        {
            return await _context.Payments.Where(p => 
                p.EventType == EventType.Seminar && seminarIds.Contains(p.EventId.Value)
                && p.Status == PaymentStatus.Completed)
                .GroupBy(s => s.EventId)
                .Select(g => new
                {
                    SeminarId = g.Key.Value,
                    Income = g.Sum(p => p.Amount)
                })
                .ToDictionaryAsync(x => x.SeminarId, x => x.Income);
        }

        #endregion

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

        private static IQueryable<UserMembershipEntity> FilterStat(IQueryable<UserMembershipEntity> query, StatAttendanceFilter filter)
        {
            if (filter.ClubIds != null)
            {
                query = query.Where(um => filter.ClubIds.Any(clubId => clubId == um.ClubId));
            }
            if (filter.GroupIds != null)
            {
                query = query.Where(um => filter.GroupIds.Any(groupId => groupId == um.GroupId));
            }
            if (filter.Grades != null)
            {
                var gradeEnums = filter.Grades.Select(EnumParser.ConvertStringToEnum<Grade>).ToList();
                query = query.Where(um => gradeEnums.Contains(um.User.Grade));
            }
            if (filter.Cities != null)
            {
                var lowerCities = filter.Cities.Select(c => c.ToLower()).ToList();
                query = query.Where(um => um.User.City != null && lowerCities.Contains(um.User.City.ToLower()));
            }
            if (filter.CoachId != null)
            {
                query = query.Where(um => um.Group.UserMemberships.Any(u => u.RoleInGroup == Role.Coach && u.Id == filter.CoachId));
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
