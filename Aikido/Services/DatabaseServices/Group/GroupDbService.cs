using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Dto.ExclusionDates;
using Aikido.Dto.Groups;
using Aikido.Dto.Schedule;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Aikido.Services.DatabaseServices.Group
{
    public class GroupDbService : IGroupDbService
    {
        private readonly AppDbContext _context;

        public GroupDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GroupEntity> GetByIdOrThrowException(long id, bool isLater = true)
        {
            var group = await _context.Groups
                .Include(g => g.MainCoach)
                .Include(g => g.Club)
                .Include(g => g.UserMemberships
                    .Where(um => um.ClosedAt == null))
                    .ThenInclude(ug => ug.User)
                .Include(g => g.Schedule
                    .Where(s => s.ClosedAt == null))
                .Include(g => g.ExclusionDates
                    .Where(e => !isLater || e.Date > DateTime.UtcNow))
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                throw new EntityNotFoundException($"Группа с Id = {id} не найдена");
            }
            return group;
        }

        public async Task<GroupEntity> GetGroupByIdAsync(long id)
        {
            return await GetByIdOrThrowException(id);
        }

        public async Task<bool> ExistsActive(long id)
        {
            return await _context.Groups
                .AnyAsync(g => g.Id == id
                && g.ClosedAt == null);
        }

        public async Task<List<GroupEntity>> GetAllActiveAsync()
        {
            return await _context.Groups
                .Include(g => g.MainCoach)
                .Include(g => g.UserMemberships
                    .Where(um => um.ClosedAt == null))
                    .ThenInclude(um => um.User)
                .Include(g => g.Club)
                .Include(g => g.Schedule)
                .Include(g => g.ExclusionDates)
                .Where(g => g.ClosedAt == null)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<List<GroupEntity>> GetGroupsByClub(long clubId)
        {
            return await _context.Groups
                .Include(g => g.MainCoach)
                .Include(g => g.Club)
                .Include(g => g.Schedule)
                .Include(g => g.UserMemberships.Where(um => um.ClosedAt == null))
                .Include(g => g.ExclusionDates)
                .Where(g => g.ClubId == clubId 
                    && g.ClosedAt == null)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<GroupEntity> CreateAsync(GroupCreationDto groupData)
        {
            var group = new GroupEntity(groupData);

            if (group.ClubId == 0 || group.ClubId == null)
                throw new ArgumentException("У группы должен быть клуб");

            _context.Groups.Add(group);

            await _context.SaveChangesAsync();

            _context.Schedule.AddRange(groupData.Schedule.Select(s => new ScheduleEntity(group.Id, s)));
            _context.ExclusionDates.AddRange(groupData.ExclusionDates.Select(e => new ExclusionDateEntity(group.Id, e)));

            await _context.SaveChangesAsync();

            return group;
        }


        public async Task UpdateAsync(long id, GroupCreationDto groupData)
        {
            var group = await GetByIdOrThrowException(id);

            group.UpdateFromJson(groupData);
  
            await UpdateSchedule(group, groupData.Schedule);
            await UpdateExclusionDates(group, groupData.ExclusionDates);

            await _context.SaveChangesAsync();
        }

        private async Task UpdateSchedule(GroupEntity group, List<ScheduleCreationDto> groupSchedule)
        {
            if (groupSchedule != null)
            {
                foreach (var schedule in group.Schedule.Where(s => s.ClosedAt == null))
                {
                    if (!groupSchedule.Any(s => s.DayOfWeek == schedule.DayOfWeek
                    && s.StartTime == schedule.StartTime
                    && s.EndTime == schedule.EndTime))
                    {
                        schedule.ClosedAt = DateTime.UtcNow;
                    }

                }

                var schedulesToCreate = new List<ScheduleEntity>();

                foreach(var schedule in groupSchedule)
                {
                    if (!group.Schedule.Where(s => s.ClosedAt == null)
                        .Any(s => s.DayOfWeek == schedule.DayOfWeek
                            && s.StartTime == schedule.StartTime
                            && s.EndTime == schedule.EndTime))
                    {
                        schedulesToCreate.Add(new(group.Id, schedule));
                    }
                }
            
                _context.UpdateRange(group.Schedule);
                await _context.AddRangeAsync(schedulesToCreate);
            }
        }

        private async Task UpdateExclusionDates(GroupEntity group, List<ExclusionDateCreationDto> groupExclusionDates)
        {
            if (groupExclusionDates != null)
            {
                foreach (var exclusionDate in group.ExclusionDates.Where(e => e.Date > DateTime.UtcNow))
                {
                    _context.Remove(exclusionDate);
                }

                var exclusionDatesToCreate = new List<ExclusionDateEntity>();

                foreach (var exclusionDate in groupExclusionDates)
                {
                    if (!group.ExclusionDates
                        .Any(s => s.Date == exclusionDate.Date
                            && s.StartTime == exclusionDate.StartTime
                            && s.EndTime == exclusionDate.EndTime))
                    {
                        exclusionDatesToCreate.Add(new(group.Id, exclusionDate));
                    }
                }

                _context.UpdateRange(group.ExclusionDates);
                await _context.AddRangeAsync(exclusionDatesToCreate);
            }
        }

        public async Task CloseAsync(long id)
        {
            await SetStatus(id, false);
        }

        public async Task RecoverAsync(long id)
        {
            await SetStatus(id, true);
        }

        private async Task SetStatus(long id, bool isActiveStatus)
        {
            var group = await GetGroupByIdAsync(id);
            if (group == null)
            {
                throw new EntityNotFoundException(nameof(GroupEntity));
            }
            group.ClosedAt = isActiveStatus? null : DateTime.UtcNow;
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var group = await GetByIdOrThrowException(id);
            var club = _context.Clubs.Find(group.ClubId);
            if (club != null)
                club.Groups.Remove(group);

            _context.Schedule.RemoveRange(group.Schedule);
            _context.ExclusionDates.RemoveRange(group.ExclusionDates);

            _context.Remove(group);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserMembershipEntity>> GetGroupActiveMembersAsync(long groupId, Role role = Role.User)
        {
            var memberships = await _context.UserMemberships
                .Include(um => um.User)
                .Where(um => um.GroupId == groupId
                    && um.RoleInGroup == role
                    && um.ClosedAt == null)
                .OrderBy(um => um.User!.LastName)
                .ThenBy(um => um.User!.FirstName)
                .ThenBy(um => um.User!.MiddleName)
                .ToListAsync();

            return memberships;
        }

        public async Task<List<UserMembershipEntity>> GetGroupMembersAsync(long groupId, Role role = Role.User)
        {
            var memberships = await _context.UserMemberships
                .Include(um => um.User)
                .Where(um => um.GroupId == groupId
                    && um.RoleInGroup == role)
                .OrderBy(um => um.User!.LastName)
                .ThenBy(um => um.User!.FirstName)
                .ThenBy(um => um.User!.MiddleName)
                .ToListAsync();

            return memberships;
        }

        public async Task RemoveAllMembersFromGroupAsync(long groupId)
        {
            var members = await _context.UserMemberships
                .Where(um => um.GroupId == groupId)
                .ToListAsync();

            foreach (var member in members)
            {
                _context.Remove(member);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetGroupMemberCountAsync(long groupId)
        {
            return await _context.UserMemberships
                .CountAsync(um => um.GroupId == groupId 
                && um.RoleInGroup == Role.User
                && um.ClosedAt == null);
        }
    }
}