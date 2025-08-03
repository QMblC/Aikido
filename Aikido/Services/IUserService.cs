using Aikido.Entities.Users;
using Aikido.Services.Base;

namespace Aikido.Services
{
    public interface IUserService : IDbService<UserEntity>
    {
    }
}
