using Aikido.Dto.Seminars;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarScheduleEntity
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public SeminarEntity? Seminar { get; set; }

        public long SeminarGroupId { get; set; }
        public SeminarGroupEntity SeminarGroup { get; set; }

        public DateTime StartTime { get; set; }
        public string Description { get; set; } = "";

        public SeminarScheduleEntity() { }

        public SeminarScheduleEntity(long seminarId, long seminarGroupId, SeminarScheduleDto schedule)
        {
            SeminarId = seminarId;

            SeminarGroupId = seminarGroupId;
            StartTime = new DateTime(
                schedule.Date.Year,
                schedule.Date.Month,
                schedule.Date.Day,
                schedule.StartTime.Hours,
                schedule.StartTime.Minutes,
                0,
                DateTimeKind.Utc);
            Description = schedule.Description;
        }
    }
}
