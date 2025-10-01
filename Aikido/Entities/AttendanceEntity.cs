using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class AttendanceEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long EventId { get; set; }
        public virtual EventEntity? Event { get; set; }

        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? Notes { get; set; }
        public AttendanceStatus? Status { get; set; }

        public AttendanceEntity() { }

        public AttendanceEntity(AttendanceDto attendanceData)
        {
            UpdateFromJson(attendanceData);
        }

        public void UpdateFromJson(AttendanceDto attendanceData)
        {
            UserId = attendanceData.UserId;
            EventId = attendanceData.EventId;
            Date = attendanceData.Date;
            IsPresent = attendanceData.IsPresent;
            CheckInTime = attendanceData.CheckInTime;
            CheckOutTime = attendanceData.CheckOutTime;
            Notes = attendanceData.Notes;
            if (!string.IsNullOrEmpty(attendanceData.Status))
                Status = EnumParser.ConvertStringToEnum<AttendanceStatus>(attendanceData.Status);
        }
    }

    public enum AttendanceStatus
    {
        Present,
        Late,
        Absent,
        Excused
    }
}