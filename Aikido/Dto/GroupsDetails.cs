namespace Aikido.Dto
{
    public class GroupDetailsDto
    {
        public string? Name { get; set; }
        public CoachDto? Coach { get; set; }
        public Dictionary<string, string>? Schedule { get; set; } // пример: "пн" → "19.00-20.30"
    }
}
