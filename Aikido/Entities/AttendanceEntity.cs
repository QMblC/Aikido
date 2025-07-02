using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class AttendanceEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public long GroupId { get; set; }
        public DateTime VisitDate { get; set; }
    }
}
