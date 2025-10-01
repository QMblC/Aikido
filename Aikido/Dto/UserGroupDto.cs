using Aikido.Entities;

namespace Aikido.Dto
{
    public class UserGroupDto : DtoBase
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public long GroupId { get; set; }
        public string? GroupName { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public bool IsActive { get; set; }
        public string RoleInGroup { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int AttendanceCount { get; set; }
        public DateTime? LastAttendanceDate { get; set; }
        public bool IsRegular { get; set; }

        public UserGroupDto() { }

        public UserGroupDto(UserGroup userGroup)
        {
            Id = userGroup.Id;
            UserId = userGroup.UserId;
            UserName = userGroup.User?.FullName;
            GroupId = userGroup.GroupId;
            GroupName = userGroup.Group?.Name;
            JoinDate = userGroup.JoinDate;
            LeaveDate = userGroup.LeaveDate;
            IsActive = userGroup.IsActive;
            RoleInGroup = userGroup.RoleInGroup.ToString();
            Notes = userGroup.Notes;
            AttendanceCount = userGroup.AttendanceCount;
            LastAttendanceDate = userGroup.LastAttendanceDate;
            IsRegular = userGroup.IsRegular;
        }
    }
}