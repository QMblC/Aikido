namespace Aikido.Dto.Seminars.Creation
{
    public class SeminarMemberCreationDto
    {
        public long UserId { get; set; }
        public long? SeminarGroupId { get; set; }
        public string? CertificationGrade { get; set; } = string.Empty;

        public bool IsSeminarPayed { get; set; }
        public decimal? SeminarPrice { get; set; }

        public bool IsBudoPassportPayed { get; set; }
        public decimal? BudoPassportPrice { get; set; }

        public bool IsAnnualFeePayed { get; set; }
        public decimal? AnnualFeePrice { get; set; }

        public bool IsCertificationPrice { get; set; }
        public decimal? CertificationPrice { get; set; }
    }
}
