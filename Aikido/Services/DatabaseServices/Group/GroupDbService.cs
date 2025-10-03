using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Exceptions;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using Microsoft.EntityFrameworkCore;

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
                .Include(g => g.Coach)
                .Include(g => g.Club)
                .Include(g => g.UserGroups)
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
                .Include(g => g.Coach)
                .Include(g => g.Club)
                .Include(g => g.Schedule)
                .Include(g => g.ExclusionDates)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<List<GroupEntity>> GetGroupsByClub(long clubId)
        {
            return await _context.Groups
                .Include(g => g.Coach)
                .Include(g => g.Club)
                .Where(g => g.ClubId == clubId && g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }

        public async Task<long> CreateAsync(GroupDto groupDto)
        {
            var group = new GroupEntity();

            group.UpdateFromJson(groupDto);

            if (group.ClubId == 0 || group.ClubId == null)
                throw new ArgumentException("ClubId is required");

            _context.Groups.Add(group);
            var club = _context.Clubs.Find(group.ClubId);
            if (club != null)
                club.Groups.Add(group);

            _context.Schedule.AddRange(group.Schedule);
            _context.ExclusionDates.AddRange(group.ExclusionDates);

            await _context.SaveChangesAsync();

            return group.Id;
        }


        public async Task UpdateAsync(long id, GroupDto groupData)
        {
            var group = await GetByIdOrThrowException(id);
            group.UpdateFromJson(groupData);
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

        public async Task<List<UserGroupEntity>> GetGroupMembersAsync(long groupId)
        {
            return await _context.UserGroups
                .Include(ug => ug.User)
                .Where(ug => ug.GroupId == groupId)
                .OrderBy(ug => ug.User!.LastName)
                .ThenBy(ug => ug.User!.FirstName)
                .ThenBy(ug => ug.User!.SecondName)
                .ToListAsync();
        }

        public async Task RemoveAllMembersFromGroupAsync(long groupId)
        {
            var members = await _context.UserGroups
                .Where(ug => ug.GroupId == groupId && ug.IsActive)
                .ToListAsync();

            foreach (var member in members)
            {
                member.IsActive = false;
                member.LeaveDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetGroupMemberCountAsync(long groupId)
        {
            return await _context.UserGroups
                .CountAsync(ug => ug.GroupId == groupId && ug.IsActive);
        }
    }
}