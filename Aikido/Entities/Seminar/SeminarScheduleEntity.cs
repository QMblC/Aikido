using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarScheduleEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public SeminarEntity? Seminar { get; set; }

        public long SeminarGroupId { get; set; }
        public SeminarGroupEntity SeminarGroup { get; set; }

        public DateTime StartTime { get; set; }

        public SeminarScheduleType Type { get; set; }

        public string Description { get; set; } = "";

        public SeminarScheduleEntity() { }

        public SeminarScheduleEntity(long seminarId, long seminarGroupId, ISeminarScheduleDto schedule)
        {
            SeminarId = seminarId;

            SeminarGroupId = seminarGroupId;
            var dateTime = new DateTime(
                schedule.Date.Year,
                schedule.Date.Month,
                schedule.Date.Day,
                schedule.StartTime.Hours,
                schedule.StartTime.Minutes,
                0);
            Type = EnumParser.ConvertStringToEnum<SeminarScheduleType>(schedule.Type);

            StartTime = dateTime;
            Description = schedule.Description;
        }
    }
}
