using Aikido.Dto.ExclusionDates;
using Aikido.Dto.Schedule;

namespace Aikido.Dto.Groups
{
    public class GroupScheduleDto : DtoBase
    {
        public List<ScheduleDto> Schedule { get; set; }
        public List<ExclusionDateDto> ExclusionDate { get; set; } 

        public GroupScheduleDto(GroupDto group)
        {
            Schedule = group.Schedule ?? new();
            ExclusionDate = group.ExclusionDates ?? new();
        }
    }
}
