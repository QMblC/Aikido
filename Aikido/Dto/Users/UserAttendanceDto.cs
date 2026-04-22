using Aikido.Dto.Attendance;
using Aikido.Entities;
using Aikido.Entities.Users;

namespace Aikido.Dto.Users
{
    public class UserAttendanceDto : UserShortDto
    {
        public List<AttendanceDto> Attendances { get; set; } = new();
        public int AttendanceCount => Attendances.Count;
        public List<ActivePeriodDto> ActivePeriods { get; set; }

        public UserAttendanceDto(UserShortDto user, List<AttendanceDto> attendances)
        {
            Id = user.Id.Value;
            LastName = user.LastName;
            FirstName = user.FirstName;
            MiddleName = user.MiddleName;
            Photo = user.Photo;
            Grade = user.Grade;

            Attendances = attendances;
        }

        public UserAttendanceDto(List<UserMembershipEntity> userMemberships, List<AttendanceDto> attendances)
        {
            var user = userMemberships.First().User;

            Id = user.Id;
            LastName = user.LastName;
            FirstName = user.FirstName;
            MiddleName = user.MiddleName;
            Photo = user.Photo;
            Grade = EnumParser.ConvertEnumToString(user.Grade);

            Attendances = attendances;
            ActivePeriods = userMemberships.Select(um => new ActivePeriodDto(um.CreateAt, um.ClosedAt)).ToList();
        }
    }
}
