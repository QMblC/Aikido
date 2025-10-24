
using Aikido.AdditionalData;

namespace Aikido.Dto.ExclusionDates
{
    public class ExclusionDateCreationDto : IExclusionDateDto
    {
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
