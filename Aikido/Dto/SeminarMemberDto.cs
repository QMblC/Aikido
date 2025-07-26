using Aikido.AdditionalData;
using Aikido.Entities;

namespace Aikido.Dto
{
    public class SeminarMemberDto
    {
        public long? Id { get; set; }
        public long? UserId { get; set; }
        public DateTime? CertificationDate { get; set; }
        public string OldGrade { get; set; }
        public string CertificationGrade { get; set; }
        public bool GradeConfirmationStatus { get; set; }
        public string? ResultStatus { get; set; }

        public SeminarMemberDto() { }

        public SeminarMemberDto(SeminarMemberEntity memberEntity)
        {
            Id = memberEntity.Id;
            UserId = memberEntity.UserId;
            CertificationDate = memberEntity.CertificationDate;
            OldGrade = EnumParser.ConvertEnumToString(memberEntity.OldGrade);
            CertificationGrade = EnumParser.ConvertEnumToString(memberEntity.CertificationGrade);
            GradeConfirmationStatus = memberEntity.GradeConfirmationStatus;
            ResultStatus = EnumParser.ConvertEnumToString(memberEntity.ResultStatus);
        }
    }
}
