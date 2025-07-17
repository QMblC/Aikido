using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class GroupService : DbService
    {
        public GroupService(AppDbContext context) : base(context) { }

        public async Task<GroupEntity> GetGroupById(long id)
        {
            var groupEntity = await context.Groups.FindAsync(id);
            if (groupEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            return groupEntity;
        }

        public async Task<bool> Contains(long id)
        {
            var groupEntity = await context.Groups.FindAsync(id);
            return groupEntity != null;
        }

        public async Task<long> CreateGroup(GroupDto groupData)
        {
            var groupEntity = new GroupEntity();
            groupEntity.UpdateFromJson(groupData);

            context.Groups.Add(groupEntity);

            await SaveDb();

            return groupEntity.Id;
        }

        public async Task UpdateGroup(long id, GroupDto newData)
        {
            var groupEntity = await GetGroupById(id);
            groupEntity.UpdateFromJson(newData);

            await SaveDb();
        }

        public async Task DeleteGroup(long id, bool saveDB = true)
        {
            var groupEntity = await context.Groups.FindAsync(id);
            if (groupEntity == null)
                throw new KeyNotFoundException($"Клуб с Id = {id} не найден.");

            context.Remove(groupEntity);

            if (saveDB)
                await SaveDb();
        }

        public async Task<List<GroupEntity>> GetGroupsByClubId(long clubId)
        {
            return await context.Groups
                .Where(g => g.ClubId == clubId)
                .ToListAsync();

        }

        public async Task<List<GroupEntity>> GetGroupsByUser(long userId)
        {
            return await context.Groups
                .Where(g => g.CoachId == userId || g.UserIds.Contains(userId))
                .ToListAsync();
        }

        public async Task AddUserToGroup(long groupId, long userId)
        {
            var groupEntity = await context.Groups.FindAsync(groupId);

            if (groupEntity == null)
            {
                throw new KeyNotFoundException($"Пользователя с Id = {userId} не существует");
            }

            groupEntity.AddUser(userId);

            await SaveDb();
        }

        public async Task DeleteUserFromGroup(long groupId, long userId)
        {
            var groupEntity = await context.Groups.FindAsync(groupId);

            if (groupEntity == null)
            {
                throw new KeyNotFoundException($"Пользователя с Id = {userId} не существует");
            }

            groupEntity.DeleteUser(userId);

            await SaveDb();
        }

        public async Task<List<GroupEntity>> GetGroups()
        {
            return await context.Groups.ToListAsync();
        }

        public async Task<List<long>> GetGroupMemberIds(long groupId)
        {
            try
            {
                return await context.Groups
                    .Where(g => g.Id == groupId)
                    .SelectMany(g => g.UserIds)
                    .ToListAsync();
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException($"Пользователь с Id = {groupId} не найден.");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }

        public async Task UpdateGroupMembers(long groupId, List<long> memberIds)
        {
            var groupEntity = context.Groups.FindAsync(groupId).Result;
            if (groupEntity == null)
            {
                throw new KeyNotFoundException($"Группа с Id = {groupId} не найдена");
            }

            groupEntity.UserIds = memberIds;

            await SaveDb();
        }
    }
}
