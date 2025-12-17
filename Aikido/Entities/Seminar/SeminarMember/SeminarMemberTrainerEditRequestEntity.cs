// Aikido/Entities/Seminar/SeminarMember/SeminarMemberTrainerEditRequestEntity.cs
using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar.SeminarMemberRequest
{
    public class SeminarMemberTrainerEditRequestEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        // Семинар и тренер (инициатор запроса)
        public long SeminarId { get; set; }
        public virtual SeminarEntity? Seminar { get; set; }

        public long TrainerId { get; set; }
        public virtual UserEntity? Trainer { get; set; }

        // Клуб, на который относятся члены
        public long ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        // Данные члена семинара
        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

        public long? CoachId { get; set; }
        public virtual UserEntity? Coach { get; set; }

        public long? SeminarGroupId { get; set; }
        public virtual SeminarGroupEntity? SeminarGroup { get; set; }

        public Grade? CertificationGrade { get; set; }

        public string? Note { get; set; }

        public TrainerEditRequestType RequestType { get; set; }
        public TrainerEditRequestStatus Status { get; set; } = TrainerEditRequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public string? ManagerComment { get; set; }

        public long? ManagerId { get; set; }
        public virtual UserEntity? Manager { get; set; }

        public bool IsApplied { get; set; } = false;
    }

    public enum TrainerEditRequestType
    {
        Add,
        Update,
        Delete
    }

    public enum TrainerEditRequestStatus
    {
        Pending,
        Approved,
        Rejected,
        Applied
    }
}
