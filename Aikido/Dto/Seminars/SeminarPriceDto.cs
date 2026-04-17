using Aikido.Entities.Seminar;
using System.ComponentModel;

namespace Aikido.Dto.Seminars
{
    public class SeminarPriceDto
    {
        public long Id { get; set; }
        public string PaymentType { get; set; }
        public string? CertificationGrade { get; set; }
        public decimal? Amount { get; set; }

        public SeminarPriceDto(SeminarPriceEntity price)
        {
            Id = price.Id;
            PaymentType = EnumParser.ConvertEnumToString(price.PaymentType);
            CertificationGrade = price.CertificationGrade == null ?
                null : EnumParser.ConvertEnumToString(price.CertificationGrade.Value);

            Amount = price.Amount;
        }
    }
}
