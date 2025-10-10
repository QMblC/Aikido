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

        public TimeSpan StartTime { get; set; }
        public string Description { get; set; } = "";

        public SeminarScheduleEntity() { }

        public SeminarScheduleEntity(long seminarId, SeminarScheduleDto schedule)
        {
            SeminarId = seminarId;
            StartTime = schedule.StartTime;
            Description = schedule.Description;
        }
    }
}
