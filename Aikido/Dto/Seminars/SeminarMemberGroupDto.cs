using Aikido.Dto.Seminars.Creation;

namespace Aikido.Dto.Seminars
{
    public class SeminarMemberGroupDto
    {
        public long CoachId { get; set; }
        public List<SeminarMemberCreationDto> Members { get; set; } = new();
    }
}
