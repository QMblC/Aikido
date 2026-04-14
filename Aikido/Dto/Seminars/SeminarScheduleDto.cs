using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public class SeminarScheduleDto : DtoBase, ISeminarScheduleDto
    {
        public long? Id { get; set; }
        public string GroupName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Type { get; set; }
        public string Description { get; set; } = "";

        public SeminarScheduleDto() { }

        public SeminarScheduleDto(SeminarScheduleEntity schedule)
        {
            Id = schedule.Id;
            GroupName = schedule.SeminarGroup.Name;
            Date = schedule.StartTime.Date;
            StartTime = schedule.StartTime.TimeOfDay;
            Description = schedule.Description;
            Type = EnumParser.ConvertEnumToString(schedule.Type);
        }
    }
}
