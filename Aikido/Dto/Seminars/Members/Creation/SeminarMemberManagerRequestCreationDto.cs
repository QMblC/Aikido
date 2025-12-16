namespace Aikido.Dto.Seminars.Members.Creation
{
    public class SeminarMemberManagerRequestCreationDto : ISeminarMemberCreation
    {
        public long UserId { get; set; }
        public long? SeminarGroupId { get; set; }
        public string? CertificationGrade { get; set; } = string.Empty;

        public long? GroupId { get; set; }
        public long? CoachId { get; set; }

        public bool IsSeminarPayed { get; set; }
        public decimal? SeminarPriceInRubles { get; set; }

        public bool IsBudoPassportPayed { get; set; }
        public decimal? BudoPassportPriceInRubles { get; set; }

        public bool IsAnnualFeePayed { get; set; }
        public decimal? AnnualFeePriceInRubles { get; set; }

        public bool IsCertificationPayed { get; set; }
        public decimal? CertificationPriceInRubles { get; set; }

        public string? Note { get; set; }
    }
}
