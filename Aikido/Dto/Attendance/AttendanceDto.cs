using Aikido.Entities;

namespace Aikido.Dto.Attendance
{
    public class AttendanceDto : DtoBase
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? UserFullName { get; set; }

        public long? GroupId { get; set; }
        public string? GroupName { get; set; }

        public DateTime Date { get; set; }


        public AttendanceDto() { }

        public AttendanceDto(AttendanceEntity attendance)
        {
            Id = attendance.Id;
            UserId = attendance.UserMembership.UserId;
            UserFullName = attendance.UserMembership?.User?.FullName;
            GroupId = attendance.UserMembership?.GroupId;
            GroupName = attendance.UserMembership.Group?.Name;
            Date = attendance.Date; 
        }
    }
}
