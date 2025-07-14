namespace Aikido.Dto
{
    public class SeminarDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public string? Location { get; set; }
        public decimal? PriceSeminarInRubles { get; set; }
        public decimal? PriceAnnualFeeRubles { get; set; }
        public decimal? PriceBudoPassportRubles { get; set; }
        public decimal? PriceAttestation5to2KyuInRubles { get; set; }
        public decimal? PriceAttestation1KyuInRubles { get; set; }
        public decimal? PriceAttestationBlackBeltInRubles { get; set; }
        public byte[]? FinalStatementFile { get; set; }
    }
}
