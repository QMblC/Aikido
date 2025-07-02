using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class SheduleEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long GroupId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
