using Aikido.Dto.Schedule;
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

        public ScheduleEntity(long groupId, IScheduleDto scheduleData)
        {
            UpdateFromJson(groupId, scheduleData);
        }

        public void UpdateFromJson(long groupId, IScheduleDto scheduleData)
        {
            GroupId = groupId;
            DayOfWeek = scheduleData.DayOfWeek;
            StartTime = scheduleData.StartTime;
            EndTime = scheduleData.EndTime;
        }
    }
}