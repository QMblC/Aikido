using Aikido.Entities.Seminar;
using System.ComponentModel;

namespace Aikido.Dto.Seminars
{
    public class SeminarPriceDto
    {
        public long Id { get; set; }
        public string PaymentType { get; set; }
        public string? CertificationPriceType { get; set; }
        public decimal? Amount { get; set; }

        public SeminarPriceDto(SeminarPriceEntity price)
        {
            Id = price.Id;
            PaymentType = EnumParser.ConvertEnumToString(price.PaymentType);
            CertificationPriceType = price.CertificationPaymentType == null ?
                null : EnumParser.ConvertEnumToString(price.CertificationPaymentType.Value);

            Amount = price.Amount;
        }
    }
}
