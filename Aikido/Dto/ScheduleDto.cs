using Aikido.Entities;

namespace Aikido.Dto
{
    public class ScheduleDto : DtoBase
    {
        public long Id { get; set; }
        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }

        public ScheduleDto() { }

        public ScheduleDto(ScheduleEntity schedule)
        {
            Id = schedule.Id;
            GroupId = schedule.GroupId;
            DayOfWeek = schedule.DayOfWeek;
            StartTime = schedule.StartTime;
            EndTime = schedule.EndTime;
            Location = schedule.Location;
            Notes = schedule.Notes;
            IsActive = schedule.IsActive;
            ValidFrom = schedule.ValidFrom;
            ValidTo = schedule.ValidTo;
        }
    }
}
