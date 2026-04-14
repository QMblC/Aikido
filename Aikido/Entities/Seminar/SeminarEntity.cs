using Aikido.AdditionalData;
using Aikido.AdditionalData.Enums;
using Aikido.Dto;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarEntity : IEvent, IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; }
        public DateTime Date { get; set; }  
        public EventType EventType { get; set; }

        public string? Description { get; set; }
        public string? Location { get; set; }

        public DateTime? RegistrationDeadline { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public long CreatorId { get; set; }
        public UserEntity Creator { get; set; }

        public virtual ICollection<UserEntity> Editors { get; set; } = new List<UserEntity>();

        public long? RegulationId { get; set; }
        public SeminarRegulationEntity? Regulation { get; set; }

        public long? FinalStatementId { get; set; }
        public virtual SeminarStatementEntity? FinalStatement { get; set; }
        public bool IsFinalStatementApplied { get; set; }

        public virtual ICollection<SeminarCoachStatementEntity> CoachStatements { get; set; } = new List<SeminarCoachStatementEntity>();
        public virtual ICollection<SeminarContactInfoEntity>? ContactInfo { get; set; } = new List<SeminarContactInfoEntity>();
        public virtual ICollection<SeminarMemberManagerRequestEntity> ManagerRequestMembers { get; set; } = new List<SeminarMemberManagerRequestEntity>();
        public virtual ICollection<SeminarMemberEntity> Members { get; set; } = new List<SeminarMemberEntity>();
        public virtual ICollection<SeminarGroupEntity>? Groups { get; set; } = new List<SeminarGroupEntity>();
        public virtual ICollection<SeminarScheduleEntity>? Schedule { get; set; } = new List<SeminarScheduleEntity>();
        public virtual ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();
        public virtual ICollection<SeminarPriceEntity>? Prices { get; set; } = new List<SeminarPriceEntity>();


        public SeminarEntity() { }

        public SeminarEntity(SeminarCreationDto seminarData)
        {
            UpdateFromJson(seminarData);
            CreatedDate = DateTime.UtcNow;
        }


        public void UpdateFromJson(SeminarCreationDto seminarData)
        {
            if (!string.IsNullOrEmpty(seminarData.Name))
                Name = seminarData.Name;

            Description = seminarData.Description;
            Date = seminarData.Date;
            Location = seminarData.Location;

            CreatorId = seminarData.CreatorId.Value;

            ContactInfo = seminarData.ContactInfo != null? seminarData.ContactInfo.Select(ci => new SeminarContactInfoEntity(Id, ci)).ToList() : null;            
            Groups = seminarData.Groups != null? seminarData.Groups.Select(s => new SeminarGroupEntity(Id, s)).ToList() : null;         
            Prices = seminarData.Prices != null ? seminarData.Prices.Select(p => new SeminarPriceEntity(Id, p)).ToList() : null;
        }
    }
}