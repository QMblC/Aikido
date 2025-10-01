using Aikido.AdditionalData;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Entities.Filters;

namespace Aikido.Services.DatabaseServices.User
{
    public interface IUserDbService
    {
        Task<UserEntity> GetByIdOrThrowException(long id);
        Task<bool> Exists(long id);
        Task<List<UserShortDto>> GetUserIdAndNamesAsync();
        Task<(List<UserDto> Users, int TotalCount)> GetUserListAlphabetAscending(int startIndex, int finishIndex, UserFilter filter);
        Task<long> CreateUser(UserDto userData);
        Task<List<long>> CreateUsers(List<UserDto> users);
        Task UpdateUser(long id, UserDto userData);
        Task UpdateUsers(List<UserDto> users);
        Task Delete(long id);

        // Новые методы для работы с many-to-many связями
        Task<List<UserClub>> GetUserClubsAsync(long userId);
        Task<List<UserGroup>> GetUserGroupsAsync(long userId);
        Task AddUserToClubAsync(long userId, long clubId, string membershipType = "Regular");
        Task RemoveUserFromClubAsync(long userId, long clubId);
        Task RemoveUserFromAllClubsAsync(long userId);
        Task AddUserToGroupAsync(long userId, long groupId, Role roleInGroup = Role.User);
        Task RemoveUserFromGroupAsync(long userId, long groupId);
        Task RemoveUserFromAllGroupsAsync(long userId);
        Task UpdateUserClubMembershipAsync(long userId, long clubId, UserClubDto membershipInfo);
        Task UpdateUserGroupRoleAsync(long userId, long groupId, UserGroupDto groupInfo);
    }
}