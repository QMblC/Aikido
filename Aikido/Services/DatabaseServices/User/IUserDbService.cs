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
        Task<bool> LoginExists(string login);
        Task<bool> ExistsActive(long id);
        Task<List<UserEntity>> GetActiveUsersAsync();
        Task<List<UserEntity>> GetArchivedUsersAsync();
        Task<(List<UserEntity> Users, int TotalCount)> GetActiveUserListAlphabetAscending(int startIndex, int finishIndex, UserFilter filter);
        Task<UserEntity> CreateUser(UserCreationDto userData);
        Task<List<UserEntity>> CreateUsers(List<UserCreationDto> users);
        Task UpdateUser(UserEntity user);
        Task UpdateUser(long id, UserCreationDto userData);
        Task UpdateUsers(List<UserDto> users);
        Task CloseAsync(long id);
        Task RecoverAsync(long id);
        Task Delete(long id);

        Task<List<UserEntity>> GetActiveManagers();

        Task UpdateUserGrade(long userId, Grade grade);
        Task UpdateUserBudoPassport(long userId, bool value);
        Task<List<UserEntity>> FindClubMemberByName(long clubId, string name);
        Task<List<UserEntity>> FindCoachMemberInClubByName(long clubId, long coachId, string name);
    }
}