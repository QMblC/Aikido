using Aikido.Dto.Seminars.Members.TrainerEditRequest;

public class TrainerRequestGroupDto
{
    public long TrainerId { get; set; }
    public string? TrainerName { get; set; }
    public long ClubId { get; set; }
    public string? ClubName { get; set; }
    public int PendingRequestsCount { get; set; }
    public List<SeminarMemberTrainerEditRequestDto> Requests { get; set; } = new ();
}