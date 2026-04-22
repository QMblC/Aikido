using Aikido.Dto.Attendance;
using Aikido.Dto.ExclusionDates;
using Aikido.Dto.Schedule;
using Aikido.Dto.Users;
using Aikido.Entities;
using Aikido.Entities.Users;

namespace Aikido.Dto.Groups
{
    public class GroupDashboardDto
    {
        public GroupShortDto Group { get; set; }
        public List<ScheduleDto>? Schedule { get; set; }
        public List<ExclusionDateDto>? ExclusionDates { get; set; }
        public List<UserAttendanceDto> Users { get; set; }

        public GroupDashboardDto(GroupDto group, List<UserShortDto> users, List<AttendanceDto> attendances)
        {
            Group = new GroupShortDto(group);
            Schedule = group.Schedule;
            ExclusionDates = group.ExclusionDates;
            Users = users
                .Select(u => new UserAttendanceDto(u, attendances.Where(a => a.UserId == u.Id).ToList()))
                .ToList();
        }

        public GroupDashboardDto(GroupEntity group, Dictionary<long, List<UserMembershipEntity>> userMemberships, List<AttendanceDto> attendances)
        {
            Group = new GroupShortDto(group);
            Schedule = group.Schedule.Select(s => new ScheduleDto(s))
                .ToList();
            ExclusionDates = group.ExclusionDates.Select(e => new ExclusionDateDto(e))
                .ToList();

            var grouppedUserMemberships = userMemberships;

            Users = grouppedUserMemberships
                .Select(pair => new UserAttendanceDto(pair.Value, 
                    attendances.Where(a => a.UserId == pair.Key).ToList()))
                .ToList();
        }
    }
}
