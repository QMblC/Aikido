using Aikido.AdditionalData.Enums;
using Aikido.Dto.Groups;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services;
using Aikido.Services.ApplicationServices;
using Aikido.Services.DatabaseServices.Club;
using Aikido.Services.DatabaseServices.Group;
using Aikido.Services.DatabaseServices.User;
using Aikido.Services.UnitOfWork;
using DocumentFormat.OpenXml.Office2010.Excel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Aikido.Application.Services
{
    public class GroupApplicationService
    {
        private readonly IGroupDbService _groupDbService;
        private readonly IUserDbService _userDbService;
        private readonly IUserMembershipDbService _userMembershipDbService;
        private readonly IClubDbService _clubDbService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ScheduleDbService _scheduleDbService;
        private readonly UserMembershipApplicationService _userMembershipApplicationService;
        private readonly AttendanceApplicationService _attendanceApplicationService;

        public GroupApplicationService(
            IGroupDbService groupDbService,
            IUserDbService userDbService,
            IUserMembershipDbService userMembershipDbService,
            IClubDbService clubDbService,
            IUnitOfWork unitOfWork,
            ScheduleDbService scheduleDbService,
            UserMembershipApplicationService userMembershipApplicationService,
            AttendanceApplicationService attendanceApplicationService)
        {
            _groupDbService = groupDbService;
            _userDbService = userDbService;
            _userMembershipDbService = userMembershipDbService;
            _clubDbService = clubDbService;
            _unitOfWork = unitOfWork;
            _scheduleDbService = scheduleDbService;
            _userMembershipApplicationService = userMembershipApplicationService;
            _attendanceApplicationService = attendanceApplicationService;
        }

        public async Task<GroupDto> GetGroupByIdAsync(long id)
        {
            var group = await _groupDbService.GetByIdOrThrowException(id);
            return new GroupDto(group);
        }

        public async Task<GroupInfoDto> GetGroupInfoAsync(long id)
        {
            var group = await _groupDbService.GetByIdOrThrowException(id);

            return new GroupInfoDto(group);
        }

        public async Task<List<GroupDto>> GetAllGroupsAsync()
        {
            var groups = await _groupDbService.GetAllActiveAsync();
            return groups.Select(g => new GroupDto(g)).ToList();
        }

        public async Task<List<GroupDto>> GetGroupsByUserAsync(long userId)
        {
            var userMemberships = await _userMembershipDbService.GetActiveUserMembershipsAsync(userId);
            return userMemberships.Where(ug => ug.Group != null)
                .Select(um => new GroupDto(um.Group!))
                .ToList();
        }
        
        public async Task<List<GroupShortDto>> GetGroupsByCoach(long coachId)
        {
            var coachGroups = await _userMembershipDbService.GetActiveUserMembershipsAsync(coachId);
            return coachGroups.Where(ug => ug.Group != null
                && ug.RoleInGroup == Role.Coach)
                .Select(ug => new GroupShortDto(ug.Group))
                .ToList();
        }

        public async Task<long> CreateGroupAsync(GroupCreationDto groupData)
        {
            await EnsureClubExists(groupData.ClubId.Value);
            EnsureMainCoachIsDedicated(groupData);

            GroupEntity group = null;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                group = await _groupDbService.CreateAsync(groupData);

                await _unitOfWork.SaveChangesAsync();

                await CreateCoaches(group, groupData.Coaches);
            });

            return group.Id;
        }

        public async Task UpdateGroupAsync(long id, GroupCreationDto groupData)
        {
            await EnsureGroupExists(id);
            await EnsureClubExists(groupData.ClubId.Value);
            EnsureMainCoachIsDedicated(groupData);

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _groupDbService.UpdateAsync(id, groupData);

                await _unitOfWork.SaveChangesAsync();

                var group = await _groupDbService.GetGroupByIdAsync(id);

                await RemoveExcessCoaches(group, groupData.Coaches);
                await CreateCoaches(group, groupData.Coaches);
            });        
        }

        private async Task RemoveExcessCoaches(GroupEntity group, List<long> newCoachIds)
        {
            var oldCoaches = group.UserMemberships
                .Where(um => um.RoleInGroup == Role.Coach && !newCoachIds.Contains(um.UserId))
                .ToList();

            if (oldCoaches.Any())
            {
                foreach (var coach in oldCoaches)
                {
                    await _userMembershipApplicationService.CloseUserMembershipAsync(coach.UserId, group.Id);
                }
            }
        }

        private async Task CreateCoaches(GroupEntity group, List<long> coaches)
        {
            foreach (var coachId in coaches)
            {
                await _userMembershipApplicationService.AddUserMembershipAsync(coachId, new UserMembershipCreationDto(group, false, true));
            }
        }

        public async Task CloseGroupAsync(long id)
        {
            var group = await _groupDbService.GetGroupByIdAsync(id);

            if (group.UserMemberships.Count(um => um.RoleInGroup == Role.User) > 0)
            {
                throw new InvalidOperationException("Необходимо удалить участников группы");
            }

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _scheduleDbService.CloseGroupSchedules(id);

                foreach(var userMembership in group.UserMemberships)
                {
                    await _userMembershipApplicationService.CloseUserMembershipAsync(userMembership.UserId, id);
                }

                await _groupDbService.CloseAsync(id);
            });    
        }

        public async Task RecoverGroupAsync(long id)
        {
            await _groupDbService.RecoverAsync(id);
        }

        public async Task DeleteGroupAsync(long id)
        {
            await EnsureGroupExists(id);

            await _groupDbService.RemoveAllMembersFromGroupAsync(id);
            await _groupDbService.DeleteAsync(id);
        }

        public async Task AddUserToGroupAsync(long userId, UserMembershipCreationShortDto dto)
        {
            var groupId = dto.GroupId;

            await EnsureGroupExists(groupId);
            await EnsureUserExists(userId);

            var group = await _groupDbService.GetGroupByIdAsync(groupId);

            if (group.ClubId == null)
            {
                throw new Exception($"У группы нет клуба");
            }
            var userMembership = new UserMembershipCreationDto(group, dto.IsMain, dto.IsCoach);

            await _userMembershipApplicationService.AddUserMembershipAsync(userId, userMembership);
        }

        public async Task RemoveUserFromGroupAsync(long groupId, long userId)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () => 
            { 
                await _userMembershipApplicationService.CloseUserMembershipAsync(userId, groupId); 
            });
        }

        public async Task<bool> GroupExistsAsync(long id)
        {
            return await _groupDbService.ExistsActive(id);
        }

        public async Task<List<UserShortDto>> GetGroupMembersAsync(long groupId)
        {
            var members = await _groupDbService.GetGroupActiveMembersAsync(groupId);
            return members.Where(m => m.User != null)
                .Select(m => new UserShortDto(m.User!))
                .ToList();
        }

        public async Task<List<UserShortDto>> GetGroupMembersAsync(long groupId, DateTime month)
        {
            var members = await _groupDbService.GetGroupActiveMembersAsync(groupId);
            return members.Where(m => m.User != null
                && m.CreateAt < month
                && (m.ClosedAt > month || m.ClosedAt == null))//??
                .Select(m => new UserShortDto(m.User!))
                .ToList();
        }

        public async Task<GroupDashboardDto> GetGroupDashboard(long groupId, DateTime date)
        {
            var group = await _groupDbService.GetByIdOrThrowException(groupId, false);

            await EnsureGroupContainsSchedule(group, date);

            var firstPricticeDate = GetGroupFirstPracticeDateInMonth(group, date);
            var lastPracticeDate = GetGroupLastPracticeDateInMonth(group, date);

            var members = await _groupDbService.GetGroupMembersAsync(groupId);

            var groupMembers = members
                .GroupBy(m => m.UserId)
                .ToDictionary(g => g.Key, g => g.ToList())
                .Where(um => IsUserAttended(firstPricticeDate, lastPracticeDate, um.Value, date))
                .ToDictionary();

            var attendances = await _attendanceApplicationService.GetAttendanceByGroup(groupId, date);

            return new GroupDashboardDto(group, groupMembers, attendances);
        }

        public async Task<GroupDashboardDto> GetUserDashboard(long groupId, long userId, DateTime date)
        {
            var group = await _groupDbService.GetByIdOrThrowException(groupId, false);
            var members = await _groupDbService.GetGroupMembersAsync(groupId);

            var firstPricticeDate = GetGroupFirstPracticeDateInMonth(group, date);
            var lastPracticeDate = GetGroupLastPracticeDateInMonth(group, date);

            var groupMembers = members
                .GroupBy(m => m.UserId)
                .ToDictionary(pair => pair.Key, g => g.ToList())
                .Where(pair => pair.Key == userId)
                .ToDictionary();

            var attendances = await _attendanceApplicationService.GetAttendanceByGroup(groupId, date);

            return new GroupDashboardDto(group, groupMembers, attendances);
        }

        private async Task EnsureGroupContainsSchedule(GroupEntity group, DateTime date)
        {
            var groupSchedules = await _scheduleDbService.GetSchedulesByGroup(group.Id);

            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            var hasActiveInMonth = groupSchedules.Any(s =>
                s.CreatedAt <= endOfMonth &&
                (s.ClosedAt == null || s.ClosedAt >= startOfMonth))
            || group.ExclusionDates.Any(e =>
                e.Type == ExclusiveDateType.Extra &&
                e.Date >= startOfMonth &&
                e.Date <= endOfMonth
            );

            if (!hasActiveInMonth)
            {
                throw new InvalidOperationException("У группы не было расписания в указанный период");
            }
        }

        private DateTime? GetGroupFirstPracticeDateInMonth(GroupEntity group, DateTime date)
        {
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            var exclusions = group.ExclusionDates
                .Where(e => e.Date >= startOfMonth && e.Date <= endOfMonth)
                .ToList();

            var extraDates = exclusions
                .Where(e => e.Type == ExclusiveDateType.Extra)
                .Select(e => e.Date)
                .ToList();

            var minorDates = exclusions
                .Where(e => e.Type == ExclusiveDateType.Minor)
                .Select(e => e.Date.Date)
                .ToHashSet();

            var scheduleDates = group.Schedule
                .Select(s =>
                {
                    var daysToAdd = ((int)s.DayOfWeek - (int)startOfMonth.DayOfWeek + 7) % 7;
                    var firstDate = startOfMonth.AddDays(daysToAdd);

                    return firstDate.Date + s.StartTime;
                })
                .Where(d => !minorDates.Contains(d.Date));

            var allDates = scheduleDates
                .Concat(extraDates)
                .ToList();

            return allDates.Any() ? allDates.Min() : null;
        }

        private DateTime? GetGroupLastPracticeDateInMonth(GroupEntity group, DateTime date)
        {
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            var exclusions = group.ExclusionDates
                .Where(e => e.Date >= startOfMonth && e.Date <= endOfMonth)
                .ToList();

            var extraDates = exclusions
                .Where(e => e.Type == ExclusiveDateType.Extra)
                .Select(e => e.Date)
                .ToList();

            var minorDates = exclusions
                .Where(e => e.Type == ExclusiveDateType.Minor)
                .Select(e => e.Date.Date)
                .ToHashSet();

            var scheduleDates = group.Schedule
                .Select(s =>
                {
                    var daysToSubtract = ((int)endOfMonth.DayOfWeek - (int)s.DayOfWeek + 7) % 7;
                    var lastDate = endOfMonth.AddDays(-daysToSubtract);

                    return lastDate.Date + s.StartTime;
                })
                .Where(d => !minorDates.Contains(d.Date));

            var allDates = scheduleDates
                .Concat(extraDates)
                .ToList();

            return allDates.Any() ? allDates.Max() : null;
        }

        private bool IsUserAttended(
            DateTime? firstPracticeDate,
            DateTime? lastPracticeDate,
            List<UserMembershipEntity> userMemberships,
            DateTime date)
        {
            if (!firstPracticeDate.HasValue || !lastPracticeDate.HasValue)
                return false;

            return userMemberships.Any(um =>
                um.CreateAt <= lastPracticeDate.Value &&
                (um.ClosedAt == null || um.ClosedAt.Value >= firstPracticeDate.Value));
        }

        private async Task EnsureUserExists(long userId)
        {
            if (!await _userDbService.ExistsActive(userId))
            {
                throw new EntityNotFoundException($"Пользователя с Id = {userId} не существует");
            }
        }

        private void EnsureMainCoachIsDedicated(GroupCreationDto groupData)
        {
            if (groupData.MainCoachId == null && groupData.Coaches.Count != 0)
            {
                throw new InvalidOperationException("Не указан главный тренер");
            }
        }

        private async Task EnsureGroupExists(long groupId)
        {
            if (!await _groupDbService.ExistsActive(groupId))
            {
                throw new EntityNotFoundException($"Группы с Id = {groupId} не существует");
            }
        }

        private async Task EnsureClubExists(long clubId)
        {
            if (!await _clubDbService.ExistsActive(clubId))
            {
                throw new EntityNotFoundException($"Группы с Id = {clubId} не существует");
            }
        }
    }
}