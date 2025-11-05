using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public class SeminarDto : DtoBase
    {
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

        public bool? IsFinalStatementApplied { get; set; }

        public long? CreatorId { get; set; }
        public string? CreatorName { get; set; }

        public bool? RegulationExists { get; set; } = false;

        public DateTime? RegistrationDeadline { get; set; }
        public DateTime? CreatedTime { get; set; }

        public List<SeminarContactInfoDto>? ContactInfo { get; set; }
        public List<SeminarScheduleDto>? Schedule { get; set; }
        public List<SeminarGroupDto>? Groups { get; set; }

        public SeminarDto() { }

        public SeminarDto(SeminarEntity seminar)
        {
            Id = seminar.Id;
            Name = seminar.Name;
            Description = seminar.Description;
            Date = seminar.Date;
            Location = seminar.Location;

            PriceSeminarInRubles = seminar.SeminarPriceInRubles;
            PriceAnnualFeeRubles = seminar.AnnualFeePriceInRubles;
            PriceBudoPassportRubles = seminar.BudoPassportPriceInRubles;
            Price5to2KyuCertificationInRubles = seminar.Certification5to2KyuPriceInRubles;
            Price1KyuCertificationInRubles = seminar.Certification1KyuPriceInRubles;
            PriceDanCertificationInRubles = seminar.CertificationDanPriceInRubles;

            CreatorId = seminar.CreatorId;
            CreatorName = seminar.Creator?.FullName;

            IsFinalStatementApplied = seminar.IsFinalStatementApplied;

            RegulationExists = seminar.RegulationId != null;

            RegistrationDeadline = seminar.RegistrationDeadline;
            CreatedTime = seminar.CreatedDate;

            if (seminar.ContactInfo != null)
                ContactInfo = seminar.ContactInfo.Select(ci => new SeminarContactInfoDto(ci)).ToList();

            if (seminar.Schedule != null)
            {
                Schedule = seminar.Schedule.Select(s => new SeminarScheduleDto(s)).ToList();
            }

            if (seminar.Groups != null)
            {
                Groups = seminar.Groups.Select(s => new SeminarGroupDto(s)).ToList();
            }
        }
    }
}
