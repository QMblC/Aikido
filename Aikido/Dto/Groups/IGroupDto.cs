using Aikido.Dto.ExclusionDates;
using Aikido.Dto.Schedule;

namespace Aikido.Dto.Groups
{
    public interface IGroupDto
    {
        public string Name { get; set; }
        public long? ClubId { get; set; }
        public string? AgeGroup { get; set; }
    }
}
