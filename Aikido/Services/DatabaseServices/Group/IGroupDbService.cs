using Aikido.AdditionalData.Enums;
using Aikido.Dto.Groups;
using Aikido.Entities;
using Aikido.Entities.Users;

namespace Aikido.Services.DatabaseServices.Group
{
    public interface IGroupDbService
    {
        Task<GroupEntity> GetByIdOrThrowException(long id);
        Task<GroupEntity> GetGroupById(long id);
        Task<bool> Exists(long id);
        Task<List<GroupEntity>> GetAllAsync();
        Task<List<GroupEntity>> GetGroupsByClub(long clubId);
        Task<long> CreateAsync(GroupCreationDto groupData);
        Task UpdateAsync(long id, GroupCreationDto groupData);
        Task DeleteAsync(long id);
        Task<List<UserMembershipEntity>> GetGroupMembersAsync(long groupId, Role role = Role.User);
        Task RemoveAllMembersFromGroupAsync(long groupId);
        Task<int> GetGroupMemberCountAsync(long groupId);
    }
}