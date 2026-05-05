using Aikido.Dto.FormerCertifications;
using Aikido.Entities;
using Aikido.Entities.Seminar.SeminarMember;

namespace Aikido.Dto.Users
{
    public class UserCertificationHistoryItemDto
    {
        public string Grade { get; set; }
        public DateTime Date { get; set; }
        public string? SeminarName { get; set; }

        public UserCertificationHistoryItemDto() { }

        public UserCertificationHistoryItemDto(SeminarMemberEntity member)
        {
            if (member.CertificationGrade == null)
            {
                throw new NotImplementedException("В список попала информация не с аттестации");
            }
            Grade = EnumParser.ConvertEnumToString(member.CertificationGrade.Value);
            Date = member.Seminar.Date;
            SeminarName = member.Seminar.Name;
                
        }

        public UserCertificationHistoryItemDto(FormerCertificationEntity certification)
        {
            Grade = EnumParser.ConvertEnumToString(certification.CertificationGrade);
            Date = certification.Date;
        }
    }
}
