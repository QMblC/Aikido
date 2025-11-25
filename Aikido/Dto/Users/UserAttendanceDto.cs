using Aikido.Dto.Attendance;

namespace Aikido.Dto.Users
{
    public class UserAttendanceDto : UserShortDto
    {
        public long UserId { get; set; }
        public List<AttendanceDto> Attendances { get; set; } = new();
        public int AttendanceCount => Attendances.Count;

        public UserAttendanceDto(UserShortDto user, List<AttendanceDto> attendances)
        {
            UserId = user.Id.Value;
            LastName = user.LastName;
            FirstName = user.FirstName;
            MiddleName = user.MiddleName;
            Photo = user.Photo;
            Grade = user.Grade;

            Attendances = attendances;
        }
    }
}
