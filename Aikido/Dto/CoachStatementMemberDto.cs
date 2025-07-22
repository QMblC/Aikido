using Aikido.Entities;
using Aikido.Services;

namespace Aikido.Dto
{
    public class CoachStatementMemberDto
    {
        public long? Id { get; set; }
        public string? Name { get; set; }

        public string? Grade { get; set; }
        public string? CertificationGrade { get; set; }

        public string? CoachName { get; set; }
        public string? ClubName { get; set; }
        public string? City { get; set; }
        public string? SeminarGroup { get; set; }
        public string? ProgramType { get; set; }

        public decimal? AnnualFee { get; set; }
        public decimal? SeminarPrice { get; set; }
        public decimal? CertificationPrice { get; set; }
        public decimal? BudoPassportPrice { get; set; }

        public bool IsBudoPassportPayed { get; set; }
        public bool IsAnnualFeePayed { get; set; }

        public CoachStatementMemberDto() { }

        public CoachStatementMemberDto(
            UserEntity user,
            ClubEntity club,
            UserEntity coach)
        {
            Id = user.Id;
            Name = user.FullName;
            Grade = EnumParser.ConvertEnumToString(user.Grade);
            CoachName = coach.FullName;
            CertificationGrade = null;
            ClubName = club.Name;
            City = club.City;
            SeminarGroup = "";
            ProgramType = EnumParser.ConvertEnumToString(user.ProgramType);
            IsBudoPassportPayed = user.HasBudoPassport;
        }

        public CoachStatementMemberDto(
            UserEntity user,
            ClubEntity club,
            SeminarEntity seminar,
            UserEntity coach)
        {
            Id = user.Id;
            Name = user.FullName;
            Grade = EnumParser.ConvertEnumToString(user.Grade);
            CoachName = coach.FullName;
            CertificationGrade = null;
            ClubName = club.Name;
            City = club.City;
            SeminarGroup = seminar.Groups[0];
            ProgramType = EnumParser.ConvertEnumToString(user.ProgramType);
            IsBudoPassportPayed = user.HasBudoPassport;
        }

        public void AddPrices(
            decimal? annualPrice,
            decimal? seminarPrice,
            decimal? certificationPrice,
            decimal? budoPassportPrice)
        {
            AnnualFee = annualPrice;
            SeminarPrice = seminarPrice;
            CertificationPrice = certificationPrice;
            BudoPassportPrice = budoPassportPrice;
        }
    }
}
