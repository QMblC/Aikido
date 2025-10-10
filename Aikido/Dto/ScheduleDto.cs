using Aikido.Entities;

namespace Aikido.Dto
{
    public class ScheduleDto : DtoBase
    {
        public long? Id { get; set; }
        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }


        public ScheduleDto() { }

        public ScheduleDto(ScheduleEntity schedule)
        {
            Id = schedule.Id;
            GroupId = schedule.GroupId;
            DayOfWeek = schedule.DayOfWeek;
            StartTime = schedule.StartTime;
            EndTime = schedule.EndTime;
        }
    }
}
