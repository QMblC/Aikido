using Aikido.AdditionalData;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Users
{
    public class UserGroupDataEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public UserEntity User { get; set; }

        public long GroupId { get; set; }
        public GroupEntity Group { get; set; }

        public Role RoleInGroup { get; set; }

        public UserGroupDataEntity() { }

        public UserGroupDataEntity(long clubId, List<long>? groupId = null)
        {
            ClubId = clubId;
            if (groupId != null)
            {
                Groups = groupId;
            }
        }
    }
}
