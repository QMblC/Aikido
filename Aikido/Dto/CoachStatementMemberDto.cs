using Aikido.Entities;

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
        public string? AgeGroup { get; set; }
        public string? ProgramType { get; set; }
        public decimal? AnnualPrice { get; set; }
        public decimal? SeminarPrice { get; set; }
        public decimal? CertificationPrice { get; set; }
        public decimal? BudoPassportPrice { get; set; }

        public CoachStatementMemberDto() { }

        public CoachStatementMemberDto(
            UserEntity user,
            ClubEntity club,
            GroupEntity group,
            UserEntity coach)
        {
            Id = user.Id;
            Name = user.FullName;
            Grade = EnumParser.ConvertEnumToString(user.Grade);
            CoachName = coach.FullName;
            CertificationGrade = null;
            ClubName = club.Name;
            City = club.City;
            AgeGroup = EnumParser.ConvertEnumToString(group.AgeGroup);
            ProgramType = EnumParser.ConvertEnumToString(user.ProgramType);
        }

        public void AddPrices(
            decimal? annualPrice,
            decimal? seminarPrice,
            decimal? certificationPrice,
            decimal? budoPassportPrice)
        {
            AnnualPrice = annualPrice;
            SeminarPrice = seminarPrice;
            CertificationPrice = certificationPrice;
            BudoPassportPrice = budoPassportPrice;
        }
    }
}
