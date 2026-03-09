using Aikido.AdditionalData.Enums;
using Aikido.Entities;

namespace Aikido.Dto.Users.Creation
{
    public class UserMembershipCreationDto
    {
        public long? ClubId { get; set; }
        public long? GroupId { get; set; }

        public bool IsMain { get; set; }

        public string RoleInGroup { get; set; } = Role.User.ToString();

        public UserMembershipCreationDto()
        {

        }

        public UserMembershipCreationDto(GroupEntity group, bool isMain, bool isCoach)
        {
            ClubId = group.ClubId;
            GroupId = group.Id;
            IsMain = isMain;
            RoleInGroup = isCoach ? EnumParser.ConvertEnumToString(Role.Coach) : EnumParser.ConvertEnumToString(Role.User);
        }
    }
}
