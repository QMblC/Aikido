using Aikido.AdditionalData;
using Aikido.Entities.Users;

namespace Aikido.Dto.Users
{
    public class UserMembershipDto : DtoBase
    {
        public long? Id { get; set; }

        public long? UserId { get; set; }
        public string? UserFullName { get; set; }

        public long? ClubId { get; set; }
        public string? ClubName { get; set; }

        public long? GroupId { get; set; }
        public string? GroupName { get; set; }

        public bool IsMain { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public DateTime? JoinDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public string RoleInGroup { get; set; } = Role.User.ToString();

        public int? AttendanceCount { get; set; }
        public DateTime? LastAttendanceDate { get; set; }

        public UserMembershipDto() { }

        public UserMembershipDto(UserMembershipEntity userMembership)
        {
            Id = userMembership.Id;
            UserId = userMembership.UserId;
            UserFullName = userMembership.User?.FullName;
            ClubId = userMembership.ClubId;
            ClubName = userMembership.Club?.Name;
            GroupId = userMembership.GroupId;
            GroupName = userMembership.Group?.Name;

            IsMain = userMembership.IsMain;
            IsActive = userMembership.IsActive;

            JoinDate = userMembership.JoinDate;
            LeaveDate = userMembership.LeaveDate;
            RoleInGroup = userMembership.RoleInGroup.ToString();
            AttendanceCount = userMembership.Attendances.Count();
            LastAttendanceDate = userMembership.Attendances.Count > 0
                ?
                userMembership.Attendances
                .OrderByDescending(a => a.Date)
                .Select(a => a.Date)
                .FirstOrDefault()
                :
                null;
        }
    }
}
