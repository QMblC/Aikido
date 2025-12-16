using Aikido.AdditionalData.Enums;

namespace Aikido.Dto.Users.Creation
{
    public class UserMembershipCreationDto
    {
        public long? ClubId { get; set; }
        public long? GroupId { get; set; }

        public bool IsActive { get; set; }
        public bool IsMain { get; set; }

        public string RoleInGroup { get; set; } = Role.User.ToString();
    }
}
