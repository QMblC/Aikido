using Aikido.AdditionalData.Enums;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities.Users;
using Aikido.Exceptions;

namespace Aikido.Services.ApplicationServices.UserMembership
{
    public interface IUserMembershipApplicationService
    {
        Task AddUserMembershipAsync(long userId, UserMembershipCreationDto dto);
        Task CloseUserMembershipAsync(long userId, long groupId);
        Task CloseExcessUserMemberships(long userId, List<UserMembershipCreationDto> newMemberships);
    }
}
