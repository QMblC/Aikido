using Aikido.Entities.Seminar;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto.Seminars
{
    public class SeminarDto : DtoBase
    {
        public long? Id { get; set; }
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


        public long? Creator { get; set; }
        public string? CreatorName { get; set; }

        public DateTime? RegistrationDeadline { get; set; }
        public DateTime? CreatedTime { get; set; }

        public string? Regulation { get; set; }

        public List<string>? ContactInfo { get; set; }//
        public List<>

        public SeminarDto() { }

        public SeminarDto(SeminarEntity seminar)
        {
            Id = seminar.Id;
            Name = seminar.Name;
            Description = seminar.Description;
            Date = seminar.Date;
            Location = seminar.Location;
        }
    }
}