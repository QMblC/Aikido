namespace Aikido.Dto.Seminars.Members
{
    public interface ISeminarMemberDataDto
    {
        public long UserId { get; set; }
        public string? UserFullName { get; set; }
        public DateTime? UserBirthday { get; set; }

        public long? SeminarId { get; set; }
        public string? SeminarName { get; set; } 
        public DateTime? SeminarDate { get; set; }

        public long? GroupId { get; set; }
        public string? GroupName { get; set; }
        public string? AgeGroup { get; set; }

        public long? CoachId { get; set; }
        public string CoachName { get; set; }

        public long? ClubId { get; set; }
        public string? ClubName { get; set; }
        public string? ClubCity { get; set; }

        public long? SeminarGroupId { get; set; }
        public string? SeminarGroupName { get; set; }

        public string? OldGrade { get; set; } 
        public string? CertificationGrade { get; set; } 

        public long? ManagerId { get; set; }
        public string? ManagerFullName { get; set; }

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
