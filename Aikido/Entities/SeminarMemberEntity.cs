using Aikido.AdditionalData;
using Aikido.Dto.Seminars;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class SeminarMemberEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public long SeminarId { get; set; }

        public string CoachName { get; set; }
        public string? ClubName { get; set; }
        public string? City { get; set; }
        public string? SeminarGroup { get; set; }
        public ProgramType ProgramType { get; set; }

        public DateTime CertificationDate { get; set; }
        public Grade OldGrade { get; set; }
        public Grade CertificationGrade { get; set; }
        public bool GradeConfirmationStatus { get; set; } = false;
        public SeminarMemberStatus ResultStatus { get; set; }

        public decimal AnnualFeePayment { get; set; }
        public decimal CertificationPayment { get; set; }
        public decimal SeminarPayment { get; set; }
        public decimal BudoPassportPayment { get; set; }

        public SeminarMemberEntity() { }

        public SeminarMemberEntity(SeminarMemberDto memberDto)
        {
            UserId = memberDto.Id.Value;
            SeminarId = memberDto.SeminarId.Value;
            CoachName = memberDto.CoachName;
            ClubName = memberDto.ClubName;
            City = memberDto.City;
            SeminarGroup = memberDto.SeminarGroup;
            ProgramType = EnumParser.ConvertStringToEnum<ProgramType>(memberDto.ProgramType);
            CertificationDate = memberDto.CertificationDate.Value;
        }
    }
}