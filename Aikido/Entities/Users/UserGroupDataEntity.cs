using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Users
{
    public class UserGroupDataEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long? ClubId { get; set; }
        public List<long> GroupId { get; set; } = new();

        public UserGroupDataEntity() { }

        public UserGroupDataEntity(long clubId, List<long>? groupId = null)
        {
            ClubId = clubId;
            if (groupId != null)
            {
                GroupId = groupId;
            }
        }
    }
}
