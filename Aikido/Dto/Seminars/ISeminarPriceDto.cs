using Aikido.AdditionalData.Enums;
using Aikido.Entities.Seminar;

namespace Aikido.Dto.Seminars
{
    public interface ISeminarPriceDto
    {
        public string PaymentType { get; set; }
        public string? CertificationPaymentType { get; set; }

        public decimal? Price { get; set; }
    }
}
