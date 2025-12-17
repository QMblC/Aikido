public class SeminarMemberTrainerEditRequestListDto
{
    public long TrainerId { get; set; }
    public long SeminarId { get; set; }
    public long ClubId { get; set; }

    public List<SeminarMemberTrainerEditRequestCreationDto> Members { get; set; } = new();
}