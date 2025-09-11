using Aikido.Entities;
using Aikido.Entities.Users;
using Aikido.Services.DatabaseServices.Base;

namespace Aikido.Services.DatabaseServices.Group
{
    public interface IGroupDbService : IDbService<GroupEntity>
    {
        Task<List<UserEntity>> GetGroupMembers(long groupId);
    }
}
