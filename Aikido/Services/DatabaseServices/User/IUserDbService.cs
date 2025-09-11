using Aikido.Entities;
using Aikido.Entities.Filters;
using Aikido.Entities.Users;
using Aikido.Services.DatabaseServices.Base;

namespace Aikido.Services.DatabaseServices.User
{
    public interface IUserDbService : IDbService<UserEntity>
    {
        Task<List<UserEntity>> GetFilteredUserListAlphabetAscending(UserFilter filter);
        Task<List<GroupEntity>> GetUserGroups(long userId);
        Task<List<UserEntity>> GetCoachStudents(long coachId);
    }
}
