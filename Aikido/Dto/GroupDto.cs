namespace Aikido.Dto
{
    public class GroupDto
    {
        public long? CoachId { get; set; }
        public List<long>? UserIds { get; set; }
        public long? ClubId { get; set; }
        public string? Name { get; set; }
        public string? AgeGroup { get; set; }
    }

}
