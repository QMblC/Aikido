using Aikido.AdditionalData;

namespace Aikido.Entities.Seminar
{
    public class SeminarMemberStartData
    {
        public long UserId { get; set; }
        public string UserFullName { get; set; }
        public string Grade { get; set; }
        public string ProgramType { get; set; }
        public decimal? SeminarToPay { get; set; }
        public decimal? BudoPassportToPay { get; set; }
        public decimal? AnnualFeeToPay { get; set; }

        public SeminarMemberStartData(UserEntity user)
        {
            UserId = user.Id;
            UserFullName = user.FullName;
            Grade = EnumParser.ConvertEnumToString(user.Grade);
            ProgramType = (user.Grade <= AdditionalData.Grade.Kyu1Child && user.Grade > AdditionalData.Grade.None)
                ? AdditionalData.ProgramType.Child.ToString() : AdditionalData.ProgramType.Adult.ToString();

        }
    }
}
