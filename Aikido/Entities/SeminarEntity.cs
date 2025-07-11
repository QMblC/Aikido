using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class SeminarEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string? Title { get; set; }
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public decimal PriceSeminarInRubles { get; set; }
        public decimal PriceAnnualFeeRubles { get; set; }
        public decimal PriceBudoPassportRubles { get; set; }
        public decimal PriceAttestation5to2KyuInRubles { get; set; }
        public decimal PriceAttestation1KyuInRubles { get; set; }
        public decimal PriceAttestationBlackBeltInRubles { get; set; }
        public byte[]? FinalStatementFile { get; set; }
    }
}