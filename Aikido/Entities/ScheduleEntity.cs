using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class ScheduleEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }


        public ScheduleEntity() { }

        public ScheduleEntity(ScheduleDto scheduleData)
        {
            UpdateFromJson(scheduleData);
        }

        public void UpdateFromJson(ScheduleDto scheduleData)
        {
            GroupId = scheduleData.GroupId;
            DayOfWeek = scheduleData.DayOfWeek;
            StartTime = scheduleData.StartTime;
            EndTime = scheduleData.EndTime;
        }
    }
}