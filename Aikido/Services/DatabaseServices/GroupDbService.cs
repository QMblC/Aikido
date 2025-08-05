using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Aikido.Services.DatabaseServices.Base;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices
{
    public class GroupDbService : DbService<GroupEntity, GroupDbService>
    {
        public GroupDbService(AppDbContext context, ILogger<GroupDbService> logger) : base(context, logger)
        {

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
                .Where(group => group.CoachId == userId || group.MemberData.Any(data => data.UserId == userId))
                .ToListAsync();
        }

        public async Task<List<UserEntity>> GetGroupMembers(long groupId)
        {
            try
            {
                var group = await GetByIdOrThrowException(groupId);

                return group.MemberData
                    .Select(data => data.User)
                    .Where(user => user != null)
                    .ToList();
            }
            catch (KeyNotFoundException ex)
            {
                throw new EntityNotFoundException($"Группа с Id = {groupId} не найден.");
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
