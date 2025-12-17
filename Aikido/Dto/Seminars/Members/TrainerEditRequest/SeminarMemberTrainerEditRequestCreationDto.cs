public class SeminarMemberTrainerEditRequestCreationDto
{

    public long UserId { get; set; }
    public long? GroupId { get; set; }
    public long? CoachId { get; set; }
    public long? SeminarGroupId { get; set; }
    public string? CertificationGrade { get; set; }
    public string? Note { get; set; }
}