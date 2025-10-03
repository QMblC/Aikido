namespace Aikido.Dto
{
    public class GroupInfoDto : DtoBase
    {
        public long? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long? CoachId { get; set; }
        public string? CoachName { get; set; }
        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? AgeGroup { get; set; }
        public List<UserShortDto>? GroupMembers { get; set; } = new();
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
        public List<ScheduleDto>? Schedule { get; set; } = new();
        public string? Description { get; set; }
        public int? MaxMembers { get; set; }
        public string? MinGrade { get; set; }
        public string? MaxGrade { get; set; }

        public GroupInfoDto() { }
    }
}