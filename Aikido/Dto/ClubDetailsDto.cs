namespace Aikido.Dto
{
    public class ClubDetailsDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public List<GroupDetailsDto> Groups { get; set; } = new();
    }
}
