namespace Aikido.Dto.Seminars.Creation
{
    public class SeminarPriceCreationDto : ISeminarPriceDto
    {
        public string PaymentType { get; set; }
        public string? CertificationGrade { get; set; }
        public decimal? Amount { get; set; }
    }
}
