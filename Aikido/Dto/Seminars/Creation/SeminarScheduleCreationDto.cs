
namespace Aikido.Dto.Seminars.Creation
{
    public class SeminarScheduleCreationDto : ISeminarScheduleDto
    {
        public string GroupName { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
