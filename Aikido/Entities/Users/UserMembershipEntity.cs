using Aikido.AdditionalData;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Users
{
    public class UserMembershipEntity : IDbEntity, IEquatable<UserMembershipEntity>
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public long GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public DateTime? LeaveDate { get; set; }
        public Role RoleInGroup { get; set; } = Role.User; 

        public int AttendanceCount { get; set; } = 0;
        public DateTime? LastAttendanceDate { get; set; }

        public UserMembershipEntity(long userId,
            long clubId,
            long groupId,
            Role roleInGroup = Role.User)
        {
            UserId = userId;
            ClubId = clubId;
            GroupId = groupId;
            RoleInGroup = roleInGroup;
        }

        public bool Equals(UserMembershipEntity? other)
        {
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Id == other.Id;
        }
    }
}
