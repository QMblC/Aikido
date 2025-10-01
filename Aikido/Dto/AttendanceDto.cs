using Aikido.Entities;

namespace Aikido.Dto
{
    public class AttendanceDto : DtoBase
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public long EventId { get; set; }
        public string? EventName { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; }

        public AttendanceDto() { }

        public AttendanceDto(AttendanceEntity attendance)
        {
            Id = attendance.Id;
            UserId = attendance.UserId;
            EventId = attendance.EventId;
            Date = attendance.Date;
            IsPresent = attendance.IsPresent;
            CheckInTime = attendance.CheckInTime;
            CheckOutTime = attendance.CheckOutTime;
            Notes = attendance.Notes;
            Status = attendance.Status?.ToString();
        }
    }
}
