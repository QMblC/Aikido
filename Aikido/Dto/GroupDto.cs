namespace Aikido.Dto
{
    public class GroupDto
    {
        public long CreatorId { get; set; }
        public long[]? UserIds { get; set; }
        public long ClubId { get; set; }
        public string? Name { get; set; }
        public int? AgeGroup { get; set; }
    }
}
