using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public class SeminarScheduleDto : DtoBase
    {
        public long? Id { get; set; }
        public string GroupName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Description { get; set; } = "";

        public SeminarScheduleDto() { }

        public SeminarScheduleDto(SeminarScheduleEntity schedule)
        {
            Id = schedule.Id;
            Date = schedule.StartTime.Date;
            StartTime = schedule.StartTime.TimeOfDay;
            Description = schedule.Description;
        }
    }
}
