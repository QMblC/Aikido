using Aikido.AdditionalData;

namespace Aikido.Dto.Users.Creation
{
    public class UserMembershipCreationDto
    {
        public long? ClubId { get; set; }
        public long? GroupId { get; set; }
        public string RoleInGroup { get; set; } = Role.User.ToString();
    }
}
