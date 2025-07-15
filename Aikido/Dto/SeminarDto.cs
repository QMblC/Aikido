using Aikido.Entities;

namespace Aikido.Dto
{
    public class SeminarDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public string? Location { get; set; }
        public string? Contacts { get; set; }
        public string? Description { get; set; }
        public decimal? PriceSeminarInRubles { get; set; }
        public decimal? PriceAnnualFeeRubles { get; set; }
        public decimal? PriceBudoPassportRubles { get; set; }
        public decimal? Price5to2KyuAttestationInRubles { get; set; }
        public decimal? Price1KyuAttestationInRubles { get; set; }
        public decimal? PriceDanAttestationInRubles { get; set; }
        public byte[]? FinalStatementFile { get; set; }

        public SeminarDto() { }

        public SeminarDto(SeminarEntity entity)
        {
            Id = entity.Id;
            Name = entity.Name;
            Date = entity.Date;
            Location = entity.Location;
            Contacts = entity.Contacts;
            Description = entity.Description;
            PriceSeminarInRubles = entity.PriceSeminarInRubles;
            PriceAnnualFeeRubles = entity.PriceAnnualFeeRubles;
            Price5to2KyuAttestationInRubles = entity.Price5to2KyuAttestationInRubles;
            Price1KyuAttestationInRubles = entity.Price1KyuAttestationInRubles;
            PriceDanAttestationInRubles = entity.PriceDanAttestationInRubles;
        }
    }
}
