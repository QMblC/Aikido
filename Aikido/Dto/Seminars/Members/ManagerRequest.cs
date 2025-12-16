
using Aikido.Dto.Users;

namespace Aikido.Dto.Seminars.Members
{
    public class ManagerRequest
    {
        public UserShortDto User { get; set; }
        public int RequestedMemberCount { get; set; }

        public ManagerRequest(UserShortDto user, int requestedMemberCount)
        {
            User = user;
            RequestedMemberCount = requestedMemberCount;
        }
    }
}
