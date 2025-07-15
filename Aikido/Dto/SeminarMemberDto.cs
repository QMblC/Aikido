using Aikido.AdditionalData;
using Aikido.Entities;

namespace Aikido.Dto
{
    public class SeminarMemberDto
    {
        public long? Id { get; set; }
        public long? UserId { get; set; }
        public DateTime? AttestationDate { get; set; }
        public string Grade { get; set; }
        public bool GradeConfirmationStatus { get; set; }
        public string? ResultStatus { get; set; }

        public SeminarMemberDto() { }

        public SeminarMemberDto(SeminarMemberEntity memberEntity)
        {
            Id = memberEntity.Id;
            UserId = memberEntity.UserId;
            AttestationDate = memberEntity.AttestationDate;
            Grade = EnumParser.ConvertEnumToString(memberEntity.AttestationGrade);
            GradeConfirmationStatus = memberEntity.GradeConfirmationStatus;
            ResultStatus = EnumParser.ConvertEnumToString(memberEntity.ResultStatus);
        }
    }
}
