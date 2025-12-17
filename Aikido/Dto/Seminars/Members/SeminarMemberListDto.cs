using Aikido.Dto.Seminars.Members.Creation;

namespace Aikido.Dto.Seminars.Members
{
    public class SeminarMemberListDto
    {
        public long CreatorId { get; set; }
        public List<SeminarMemberCreationDto> Members { get; set; } = new();
    }
}
