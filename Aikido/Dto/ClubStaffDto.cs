using Aikido.AdditionalData.Enums;
using Aikido.Dto.Users;
using Aikido.Entities;

namespace Aikido.Dto
{
    public class ClubStaffDto : UserShortDto
    {
        public string RoleInClub {  get; set; }

        public ClubStaffDto(UserEntity user, Role roleInClub) : base(user)
        {
            RoleInClub = EnumParser.ConvertEnumToString(roleInClub);
        }
    }
}
