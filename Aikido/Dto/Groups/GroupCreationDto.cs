using Aikido.Dto.ExclusionDates;
using Aikido.Dto.Schedule;

namespace Aikido.Dto.Groups
{
    public class GroupCreationDto : IGroupDto
    {
        public string Name { get; set; }
        public long? CoachId { get; set; }
        public long? ClubId { get; set; }
        public string? AgeGroup { get; set; }
        public List<ScheduleCreationDto>? Schedule { get; set; }
        public List<ExclusionDateCreationDto> ExclusionDates { get; set; }
    }
}
