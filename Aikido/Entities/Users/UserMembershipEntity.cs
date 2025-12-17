using Aikido.AdditionalData.Enums;
using Aikido.Dto.Users.Creation;
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

        public bool IsMain { get; set; }
        public bool IsActive { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public DateTime? LeaveDate { get; set; }
        public Role RoleInGroup { get; set; } = Role.User;

        public virtual ICollection<AttendanceEntity> Attendances { get; set; } = new List<AttendanceEntity>();

        public UserMembershipEntity(long userId, UserMembershipCreationDto userMembership)
        {
            UserId = userId;
            ClubId = userMembership.ClubId.Value;
            GroupId = userMembership.GroupId.Value;

            IsActive = userMembership.IsActive;
            IsMain = userMembership.IsMain;

            RoleInGroup = EnumParser.ConvertStringToEnum<Role>(userMembership.RoleInGroup);
        }

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
