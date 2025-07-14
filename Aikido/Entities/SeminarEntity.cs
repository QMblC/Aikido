using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class SeminarEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public string? Location { get; set; }
        public decimal? PriceSeminarInRubles { get; set; }
        public decimal? PriceAnnualFeeRubles { get; set; }
        public decimal? PriceBudoPassportRubles { get; set; }
        public decimal? PriceAttestation5to2KyuInRubles { get; set; }
        public decimal? PriceAttestation1KyuInRubles { get; set; }
        public decimal? PriceAttestationBlackBeltInRubles { get; set; }
        public byte[]? FinalStatementFile { get; set; }

        public SeminarEntity() { }

        public SeminarEntity(SeminarDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            Id = dto.Id ?? 0;
            Name = dto.Name;
            Date = dto.Date ?? DateTime.MinValue;
            Location = dto.Location;
            PriceSeminarInRubles = dto.PriceSeminarInRubles ?? null;
            PriceAnnualFeeRubles = dto.PriceAnnualFeeRubles ?? null;
            PriceBudoPassportRubles = dto.PriceBudoPassportRubles ?? null;
            PriceAttestation5to2KyuInRubles = dto.PriceAttestation5to2KyuInRubles ?? null;
            PriceAttestation1KyuInRubles = dto.PriceAttestation1KyuInRubles ?? null;
            PriceAttestationBlackBeltInRubles = dto.PriceAttestationBlackBeltInRubles ?? null;
            FinalStatementFile = dto.FinalStatementFile;
        }

        public void UpdateFromJson(SeminarDto seminarNewData)
        {
            if (seminarNewData.Name != null)
                Name = seminarNewData.Name;

            if (seminarNewData.Date.HasValue)
                Date = seminarNewData.Date.Value;

            if (seminarNewData.Location != null)
                Location = seminarNewData.Location;

            if (seminarNewData.PriceSeminarInRubles.HasValue)
                PriceSeminarInRubles = seminarNewData.PriceSeminarInRubles.Value;

            if (seminarNewData.PriceAnnualFeeRubles.HasValue)
                PriceAnnualFeeRubles = seminarNewData.PriceAnnualFeeRubles.Value;

            if (seminarNewData.PriceBudoPassportRubles.HasValue)
                PriceBudoPassportRubles = seminarNewData.PriceBudoPassportRubles.Value;

            if (seminarNewData.PriceAttestation5to2KyuInRubles.HasValue)
                PriceAttestation5to2KyuInRubles = seminarNewData.PriceAttestation5to2KyuInRubles.Value;

            if (seminarNewData.PriceAttestation1KyuInRubles.HasValue)
                PriceAttestation1KyuInRubles = seminarNewData.PriceAttestation1KyuInRubles.Value;

            if (seminarNewData.PriceAttestationBlackBeltInRubles.HasValue)
                PriceAttestationBlackBeltInRubles = seminarNewData.PriceAttestationBlackBeltInRubles.Value;

            if (seminarNewData.FinalStatementFile != null)
                FinalStatementFile = seminarNewData.FinalStatementFile;
        }
    }
}
