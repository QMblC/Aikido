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
        public string? Schedule { get; set; }
        public string? Contacts { get; set; }
        public string? Description { get; set; } = "";
        public decimal? PriceSeminarInRubles { get; set; }
        public decimal? PriceAnnualFeeRubles { get; set; }
        public decimal? PriceBudoPassportRubles { get; set; }
        public decimal? Price5to2KyuCertificationInRubles { get; set; }
        public decimal? Price1KyuCertificationInRubles { get; set; }
        public decimal? PriceDanCertificationInRubles { get; set; }
        public byte[]? FinalStatementFile { get; set; }

        public SeminarEntity() { }

        public SeminarEntity(SeminarDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            Name = dto.Name;
            Date = dto.Date ?? DateTime.MinValue;
            Location = dto.Location ?? "";
            Schedule = dto.Schedule ?? "";
            Contacts = dto.Contacts ?? "";
            Description = dto.Description ?? "";
            PriceSeminarInRubles = dto.PriceSeminarInRubles ?? 0;
            PriceAnnualFeeRubles = dto.PriceAnnualFeeRubles ?? 0;
            PriceBudoPassportRubles = dto.PriceBudoPassportRubles ?? 0;
            Price5to2KyuCertificationInRubles = dto.Price5to2KyuCertificationInRubles ?? 0;
            Price1KyuCertificationInRubles = dto.Price1KyuCertificationInRubles ?? 0;
            PriceDanCertificationInRubles = dto.PriceDanCertificationInRubles ?? 0;
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

            if (seminarNewData.Schedule != null)
                Schedule = seminarNewData.Schedule;

            if (seminarNewData.Contacts != null)
                Contacts = seminarNewData.Contacts;

            if (seminarNewData.Description != null)

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

            if (seminarNewData.FinalStatementFile != null)
                FinalStatementFile = seminarNewData.FinalStatementFile;
        }
    }
}
