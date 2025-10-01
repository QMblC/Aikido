using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class StatementEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string? Title { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedDate { get; set; }
        public StatementStatus? Status { get; set; }
        public StatementType? Type { get; set; }

        public long? UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long? ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public string? Notes { get; set; }
        public string? FilePath { get; set; }

        public StatementEntity() { }
    }

    public enum StatementStatus
    {
        Draft,
        Submitted,
        Approved,
        Rejected
    }

    public enum StatementType
    {
        Certification,
        Tournament,
        Seminar,
        Other
    }
}