using Aikido.Entities;

namespace Aikido.Dto
{
    public class SeminarDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public string? Location { get; set; }
        public string? Schedule { get; set; }
        public string? Contacts { get; set; }
        public string? Description { get; set; }
        public decimal? PriceSeminarInRubles { get; set; }
        public decimal? PriceAnnualFeeRubles { get; set; }
        public decimal? PriceBudoPassportRubles { get; set; }
        public decimal? Price5to2KyuCertificationInRubles { get; set; }
        public decimal? Price1KyuCertificationInRubles { get; set; }
        public decimal? PriceDanCertificationInRubles { get; set; }
        public byte[]? FinalStatementFile { get; set; }

        public SeminarDto() { }

        public SeminarDto(SeminarEntity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Date = entity.Date;
            Location = entity.Location;
            Schedule = entity.Schedule;
            Contacts = entity.Contacts;
            Description = entity.Description;
            PriceSeminarInRubles = entity.PriceSeminarInRubles;
            PriceAnnualFeeRubles = entity.PriceAnnualFeeRubles;
            Price5to2KyuCertificationInRubles = entity.Price5to2KyuCertificationInRubles;
            Price1KyuCertificationInRubles = entity.Price1KyuCertificationInRubles;
            PriceDanCertificationInRubles = entity.PriceDanCertificationInRubles;
        }
    }
}
