using Aikido.Dto.Seminars;
using Aikido.Entities.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aikido.Entities.Seminar
{
    public class SeminarEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }

        public string? Location { get; set; }
        public string? Description { get; set; } = "";

        public List<SeminarScheduleEntity> Schedule { get; set; } = [];
        public List<SeminarContactEntity> Contacts { get; set; } = []; 
        public List<SeminarGroupEntity> Groups { get; set; } = [];

        public List<SeminarMemberEntity> Members { get; set; } = [];


        public decimal? PriceSeminarInRubles { get; set; }
        public decimal? PriceAnnualFeeRubles { get; set; }
        public decimal? PriceBudoPassportRubles { get; set; }
        public decimal? Price5to2KyuCertificationInRubles { get; set; }
        public decimal? Price1KyuCertificationInRubles { get; set; }
        public decimal? PriceDanCertificationInRubles { get; set; }

        public List<StatementEntity> CoachStatements { get; set; } = [];
        public byte[]? Regulation { get; set; }

        public byte[]? FinalStatementFile { get; set; }
        public bool IsFinalStatementApplied { get; set; }

        public DateTime? CreationDate { get; set; }

        [ForeignKey(nameof(Creator))]
        public long? CreatorId { get; set; }
        public UserEntity? Creator { get; set; }

        public SeminarEntity() { }

        public SeminarEntity(SeminarDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            Name = dto.Name;

            if (dto.Date.HasValue)
                Date = DateTime.SpecifyKind(dto.Date.Value, DateTimeKind.Utc);

            Location = dto.Location ?? "";
            Schedule = dto.Schedule ?? [];
            Contacts = dto.Contacts ?? [];
            Description = dto.Description ?? "";
            Groups = dto.Groups ?? [];

            PriceSeminarInRubles = dto.PriceSeminarInRubles ?? 0;
            PriceAnnualFeeRubles = dto.PriceAnnualFeeRubles ?? 0;
            PriceBudoPassportRubles = dto.PriceBudoPassportRubles ?? 0;
            Price5to2KyuCertificationInRubles = dto.Price5to2KyuCertificationInRubles ?? 0;
            Price1KyuCertificationInRubles = dto.Price1KyuCertificationInRubles ?? 0;
            PriceDanCertificationInRubles = dto.PriceDanCertificationInRubles ?? 0;

            if (dto.CreationDate.HasValue)
                CreationDate = DateTime.SpecifyKind(dto.CreationDate.Value, DateTimeKind.Utc);

            CreatorId = dto.CreatorId ?? null;
            Regulation = dto.Regulation != null ? Convert.FromBase64String(dto.Regulation) : null;
            CoachStatements = [];
        }

        public void UpdateFromJson(SeminarDto seminarNewData)
        {
            if (seminarNewData.Name != null)
                Name = seminarNewData.Name;

            if (seminarNewData.Date.HasValue)
                Date = seminarNewData.Date.Value;

            if (seminarNewData.Location != null)
                Location = seminarNewData.Location;

            if (seminarNewData.Schedule != null)
                Schedule = seminarNewData.Schedule;

            if (seminarNewData.Contacts != null)
                Contacts = seminarNewData.Contacts;

            if (seminarNewData.Groups != null)
                Groups = seminarNewData.Groups;

            if (seminarNewData.Description != null)
                Description = seminarNewData.Description;

            if (seminarNewData.PriceSeminarInRubles.HasValue)
                PriceSeminarInRubles = seminarNewData.PriceSeminarInRubles.Value;

            if (seminarNewData.PriceAnnualFeeRubles.HasValue)
                PriceAnnualFeeRubles = seminarNewData.PriceAnnualFeeRubles.Value;

            if (seminarNewData.PriceBudoPassportRubles.HasValue)
                PriceBudoPassportRubles = seminarNewData.PriceBudoPassportRubles.Value;

            if (seminarNewData.Price5to2KyuCertificationInRubles.HasValue)
                Price5to2KyuCertificationInRubles = seminarNewData.Price5to2KyuCertificationInRubles.Value;

            if (seminarNewData.Price1KyuCertificationInRubles.HasValue)
                Price1KyuCertificationInRubles = seminarNewData.Price1KyuCertificationInRubles.Value;

            if (seminarNewData.PriceDanCertificationInRubles.HasValue)
                PriceDanCertificationInRubles = seminarNewData.PriceDanCertificationInRubles.Value;

            if (seminarNewData.CreationDate != null)
                CreationDate = seminarNewData.CreationDate;

            CreatorId = seminarNewData.CreatorId;

            Regulation = seminarNewData.Regulation != null ? Convert.FromBase64String(seminarNewData.Regulation) : null;
        }
    }
}
