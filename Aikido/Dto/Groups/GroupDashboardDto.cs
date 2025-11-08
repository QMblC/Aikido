using Aikido.Dto.Attendance;
using Aikido.Dto.ExclusionDates;
using Aikido.Dto.Schedule;
using Aikido.Dto.Users;
using Aikido.Entities;

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
    }
}
