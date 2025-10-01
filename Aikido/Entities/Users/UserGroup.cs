using Aikido.AdditionalData;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class UserGroup : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public DateTime? LeaveDate { get; set; }
        public bool IsActive { get; set; } = true;
        public Role RoleInGroup { get; set; } = Role.User; // Student, Assistant, etc.
        public string? Notes { get; set; }

        // Дополнительные поля для группы
        public int AttendanceCount { get; set; } = 0;
        public DateTime? LastAttendanceDate { get; set; }
        public bool IsRegular { get; set; } = true; // Regular or substitute

        public UserGroup() { }

        public UserGroup(long userId, long groupId, Role roleInGroup = Role.User)
        {
            UserId = userId;
            GroupId = groupId;
            RoleInGroup = roleInGroup;
            JoinDate = DateTime.UtcNow;
            IsActive = true;
        }
    }
}