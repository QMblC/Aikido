namespace Aikido.Dto.Seminars.Creation
{
    public class SeminarMemberCreationDto
    {
        public long UserId { get; set; }
        public long? SeminarGroupId { get; set; }
        public string? CertificationGrade { get; set; } = string.Empty;
    }
}
