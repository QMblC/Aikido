using Aikido.AdditionalData.Enums;
using Aikido.Dto.Users.Creation;
using Aikido.Entities;
using Aikido.Entities.Users;

namespace Aikido.Services.DatabaseServices.User
{
    public interface IUserMembershipDbService
    {
        Task<List<UserMembershipEntity>> GetActiveUserMembershipsAsync(long userId);
        UserMembershipEntity GetActiveUserMembership(long userId, long groupId);
        UserMembershipEntity GetMainUserMembership(long userId);
        Task RemoveUserMembershipAsync(long userId, long groupId);
        Task RemoveUserMemberships(long userId);
        

        Task CloseUserMembershipAsync(long userId, long groupId);
        Task RecoverUserMembershipAsync(long userId, long groupId);

        Task<long> CreateUserMembershipAsync(long userId, UserMembershipCreationDto userMembership);
        Task UpdateUserMembershipAsync(UserMembershipEntity userMembership);
        Task UpdateUserMembershipAsync(long userId, UserMembershipCreationDto userMembership);
        Task<bool> UserMembershipExists(long userId, long groupId);
        Task<List<UserMembershipEntity>> GetActiveUserMembershipsAsUserAsync(long userId);
    }
}
