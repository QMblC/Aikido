using Aikido.AdditionalData.Enums;

namespace Aikido.Dto.Users.Creation
{
    public class UserMembershipCreationShortDto
    {
        public long GroupId { get; set; }
        public bool IsMain { get; set; }
        public bool IsCoach { get; set; }
    }
}
