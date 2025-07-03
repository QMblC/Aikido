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
        public decimal PriceSeminar { get; set; }
        public decimal PriceAnnualFee { get; set; }
        public decimal PriceBudoPassport { get; set; }
        public decimal PriceAttestation5to2Kyu { get; set; }
        public decimal PriceAttestation1Kyu { get; set; }
        public decimal PriceAttestationBlackBelt { get; set; }
        public byte[]? FinalStatementFile { get; set; }
    }
}