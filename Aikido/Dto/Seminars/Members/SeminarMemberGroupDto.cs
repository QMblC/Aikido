namespace Aikido.Dto.Seminars.Members
{
    public class SeminarMemberGroupDto
    {
        public long CoachId { get; set; }
        public List<SeminarMemberCreationDto> Members { get; set; } = new();
    }
}
