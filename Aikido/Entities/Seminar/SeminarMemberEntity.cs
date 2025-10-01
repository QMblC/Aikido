using Aikido.AdditionalData;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarMemberEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public virtual SeminarEntity? Seminar { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public SeminarMemberStatus Status { get; set; } = SeminarMemberStatus.None;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public DateTime? PaymentDate { get; set; }
        public decimal? Amount { get; set; }
        public bool IsPaid { get; set; }
        public string? Notes { get; set; }
        public string? SpecialRequirements { get; set; }
        public bool NeedsAccommodation { get; set; }
        public string? EmergencyContact { get; set; }

        public SeminarMemberEntity() { }
    }
}