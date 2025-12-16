using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Dto.Groups;
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

        public async Task<GroupEntity> GetByIdOrThrowException(long id)
        {
            var group = await _context.Groups
                .Include(g => g.Club)
                .Include(g => g.UserMemberships)
                    .ThenInclude(ug => ug.User)
                .Include(g => g.Schedule)
                .Include(g => g.ExclusionDates)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
            {
                throw new EntityNotFoundException($"Группа с Id = {id} не найдена");
            }
            return group;
        }

        public async Task<GroupEntity> GetGroupById(long id)
        {
            return await GetByIdOrThrowException(id);
        }

        public async Task<bool> Exists(long id)
        {
            return await _context.Groups.AnyAsync(g => g.Id == id);
        }

        public async Task<List<GroupEntity>> GetAllAsync()
        {
            return await _context.Groups
                .Include(g => g.Club)
                .Include(g => g.Schedule)
                .Include(g => g.ExclusionDates)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<List<GroupEntity>> GetGroupsByClub(long clubId)
        {
            return await _context.Groups
                .Include(g => g.Club)
                .Include(g => g.Schedule)
                .Include(g => g.ExclusionDates)
                .Where(g => g.ClubId == clubId && g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<long> CreateAsync(GroupCreationDto groupData)
        {
            var group = new GroupEntity(groupData);

            if (group.ClubId == 0 || group.ClubId == null)
                throw new ArgumentException("ClubId is required");

            _context.Groups.Add(group);

            var club = _context.Clubs.Find(group.ClubId);
            if (club != null)
                club.Groups.Add(group);

            _context.Schedule.AddRange(group.Schedule);
            _context.ExclusionDates.AddRange(group.ExclusionDates);

            await _context.SaveChangesAsync();

            group.UpdateSchedule(groupData);

            if (groupData.Coaches != null && groupData.Coaches.Count > 0)
            {
                await SetCoachesAsync(group, groupData);
            }

            await _context.SaveChangesAsync();

            return group.Id;
        }


        public async Task UpdateAsync(long id, GroupCreationDto groupData)
        {
            var group = await GetByIdOrThrowException(id);

            await RemoveExcessCoaches(group, groupData);

            if (groupData.Coaches != null && groupData.Coaches.Count > 0)
            {
                await SetCoachesAsync(group, groupData);
            }

            group.UpdateFromJson(groupData);

            if (groupData.Schedule != null)
            {
                foreach (var schedule in group.Schedule)
                {
                    _context.Remove(schedule);
                }
            }

            if (groupData.ExclusionDates != null)
            {
                foreach (var exclusionDate in group.ExclusionDates)
                {
                    _context.Remove(exclusionDate);
                }
            }

            group.UpdateSchedule(groupData);

            await _context.SaveChangesAsync();
        }

        private async Task RemoveExcessCoaches(GroupEntity group, GroupCreationDto newGroupData)
        {
            var newCoachIds = newGroupData.Coaches.Select(c => c.Id).ToList();

            var oldCoaches = _context.UserMemberships
                .Where(um => um.GroupId == group.Id && newCoachIds.Contains(um.UserId))
                .ToList();

            if (oldCoaches.Any())
            {
                _context.RemoveRange(oldCoaches);
                await _context.SaveChangesAsync();
            }
        }


        private async Task SetCoachesAsync(GroupEntity group, GroupCreationDto newGroupData)
        {
            var coaches = new List<UserMembershipEntity>();

            foreach (var coach in newGroupData.Coaches)
            {
                var existingMembership = group.UserMemberships.FirstOrDefault(um => um.UserId == coach.Id);
                if (existingMembership == null)
                {
                    coaches.Add(new UserMembershipEntity(coach.Id.Value, newGroupData.ClubId.Value, group.Id, Role.Coach));
                }
                else
                {
                    existingMembership.RoleInGroup = Role.Coach;
                }
            }

            if (coaches.Any())
            {
                await _context.UserMemberships.AddRangeAsync(coaches);
            }

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
                && um.RoleInGroup == Role.User);
        }
    }
}