using Aikido.Dto.Seminars.Members.Creation;

namespace Aikido.Dto.Seminars.Members
{
    public class SeminarMemberManagerRequestListDto
    {
        public long ManagerId { get; set; }
        public long ClubId { get; set; }
        public List<SeminarMemberManagerRequestCreationDto> Members { get; set; } = new();
    }
}
