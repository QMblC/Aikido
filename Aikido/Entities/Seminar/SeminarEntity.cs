using Aikido.Dto;
using Aikido.Dto.Seminars;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public string? Description { get; set; }
        public string? Location { get; set; }

        public decimal? PriceSeminarInRubles { get; set; }
        public decimal? PriceAnnualFeeRubles { get; set; }
        public decimal? PriceBudoPassportRubles { get; set; }
        public decimal? Price5to2KyuCertificationInRubles { get; set; }
        public decimal? Price1KyuCertificationInRubles { get; set; }
        public decimal? PriceDanCertificationInRubles { get; set; }

        public DateTime? RegistrationDeadline { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public long CreatorId { get; set; }
        public UserEntity Creator { get; set; }

        public long RegulationId { get; set; }
        public SeminarRegulationEntity Regulation { get; set; }

        public long? FinalStatementId { get; set; }
        public virtual SeminarStatementEntity? FinalStatement { get; set; }
        public bool IsFinalStatementApplied { get; set; }

        public virtual ICollection<SeminarCoachStatementEntity> CoachStatements { get; set; } = new List<SeminarCoachStatementEntity>();
        public virtual ICollection<SeminarContactInfoEntity>? ContactInfo { get; set; } = new List<SeminarContactInfoEntity>();
        public virtual ICollection<SeminarMemberEntity> Members { get; set; } = new List<SeminarMemberEntity>();
        public virtual ICollection<SeminarGroupEntity> Groups { get; set; } = new List<SeminarGroupEntity>();
        public virtual ICollection<SeminarScheduleEntity> Schedule { get; set; } = new List<SeminarScheduleEntity>();

        public SeminarEntity() { }

        public SeminarEntity(SeminarDto seminarData)
        {
            UpdateFromJson(seminarData);
        }

        public void UpdateFromJson(SeminarDto seminarData)
        {
            if (!string.IsNullOrEmpty(seminarData.Name))
                Name = seminarData.Name;
            Description = seminarData.Description;
            Date = seminarData.Date;
            Location = seminarData.Location;
        }
    }
}