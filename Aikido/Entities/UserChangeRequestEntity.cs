using System.ComponentModel.DataAnnotations;
using Aikido.AdditionalData;

namespace Aikido.Entities
{
    public class UserChangeRequestEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public ChangeRequestType RequestType { get; set; }

        public long RequestedById { get; set; }
        public virtual UserEntity RequestedBy { get; set; }

        public long? TargetUserId { get; set; }
        public virtual UserEntity? TargetUser { get; set; }

        public string? UserDataJson { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
    }

    public enum ChangeRequestType
    {
        Create,
        Update,
        Delete
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Applied   
    }
}
