using Aikido.AdditionalData;
using Aikido.Dto;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class SeminarMemberEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime CertificationDate { get; set; }
        public Grade OldGrade { get; set; }
        public Grade CertificationGrade { get; set; }
        public bool GradeConfirmationStatus { get; set; } = false;
        public SeminarMemberStatus ResultStatus { get; set; }

        public SeminarMemberEntity() { }

        public SeminarMemberEntity(SeminarMemberDto memberDto)
        {
            UpdateFromJson(memberDto);
        }

        public void UpdateFromJson(SeminarMemberDto memberDto)
        {
            if (memberDto.UserId != null)
            {
                UserId = memberDto.UserId.Value;
            }
            if (memberDto.CertificationDate != null)
            {
                CertificationDate = memberDto.CertificationDate.Value;
            }
            OldGrade = EnumParser.ConvertStringToEnum<Grade>(memberDto.OldGrade);
            CertificationGrade = EnumParser.ConvertStringToEnum<Grade>(memberDto.CertificationGrade);
            GradeConfirmationStatus = memberDto.GradeConfirmationStatus;
            ResultStatus = EnumParser.ConvertStringToEnum<SeminarMemberStatus>(memberDto.ResultStatus);
        }
    }
}