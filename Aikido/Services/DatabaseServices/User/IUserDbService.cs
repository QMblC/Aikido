using Aikido.AdditionalData.Enums;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities;
using Aikido.Entities.Filters;
using Aikido.Entities.Users;

namespace Aikido.Services.DatabaseServices.User
{
    public interface IUserDbService
    {
        Task<UserEntity> GetByIdOrThrowException(long id);
        Task<bool> Exists(long id);
        Task<List<UserShortDto>> GetUserIdAndNamesAsync();
        Task<List<UserEntity>> GetCoachStudentByName(long coachId, string name);
        Task<(List<UserDto> Users, int TotalCount)> GetUserListAlphabetAscending(int startIndex, int finishIndex, UserFilter filter);
        Task<long> CreateUser(UserCreationDto userData);
        Task<List<long>> CreateUsers(List<UserCreationDto> users);
        Task UpdateUser(long id, UserCreationDto userData);
        Task UpdateUsers(List<UserDto> users);
        Task Delete(long id);

        Task<List<UserEntity>> GetManagers();

        Task<List<UserMembershipEntity>> GetUserMembershipsAsync(long userId);
        UserMembershipEntity GetUserMembership(long userId, long groupId);
        UserMembershipEntity GetMainUserMembership(long userId);
        Task AddUserMembershipAsync(long userId, UserMembershipCreationDto userMembership);
        Task RemoveUserMembershipAsync(long userId, long groupId);
        Task RemoveUserMemberships(long userId);
        Task UpdateUserGrade(long userId, Grade grade);
        Task UpdateUserBudoPassport(long userId, bool value);
        Task<List<UserEntity>> FindClubMemberByName(long clubId, string name);
    }
}