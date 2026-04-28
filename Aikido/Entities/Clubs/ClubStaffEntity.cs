using Aikido.AdditionalData.Enums;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Clubs
{
    public class ClubStaffEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public bool IsFalse { get; set; }

        public ClubStaffEntity(long clubId, long userId, bool isFalse = false)
        {
            ClubId = clubId;
            UserId = userId;
            IsFalse = isFalse;
        }
    }
}
