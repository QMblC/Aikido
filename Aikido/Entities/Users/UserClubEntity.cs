using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class UserClubEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public DateTime JoinDate { get; set; } = DateTime.UtcNow;
        public DateTime? LeaveDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Дополнительные поля для более детальной информации
        public string? MembershipType { get; set; } // Regular, Trial, VIP, etc.
        public decimal? MembershipFee { get; set; }
        public DateTime? LastPaymentDate { get; set; }

        public UserClubEntity() { }

        public UserClubEntity(long userId, long clubId)
        {
            UserId = userId;
            ClubId = clubId;
            JoinDate = DateTime.UtcNow;
            IsActive = true;
        }
    }
}