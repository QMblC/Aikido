using Aikido.AdditionalData;

namespace Aikido.Dto.Users
{
    public class UserMembershipCreationDto
    {
        public long? ClubId { get; set; }
        public long? GroupId { get; set; }
        public string RoleInGroup { get; set; } = Role.User.ToString();
    }
}
