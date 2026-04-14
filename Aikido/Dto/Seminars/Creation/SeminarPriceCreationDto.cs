namespace Aikido.Dto.Seminars.Creation
{
    public class SeminarPriceCreationDto : ISeminarPriceDto
    {
        public string PaymentType { get; set; }
        public string? CertificationPaymentType { get; set; }
        public decimal? Price { get; set; }
    }
}
