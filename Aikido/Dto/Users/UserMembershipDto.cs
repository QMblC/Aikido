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
        public bool? GroupContainsCoach { get; set; }

        public DateTime? JoinDate { get; set; }
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
            GroupContainsCoach = userMembership.Group != null ? userMembership.Group.CoachId != null : false;
            JoinDate = userMembership.JoinDate;
            RoleInGroup = userMembership.RoleInGroup.ToString();
            AttendanceCount = userMembership.AttendanceCount;
            LastAttendanceDate = userMembership.LastAttendanceDate;
        }
    }
}
