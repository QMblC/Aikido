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
        public DateTime AttestationDate { get; set; }
        public Grade AttestationGrade { get; set; }
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
            if (memberDto.AttestationDate != null)
            {
                AttestationDate = memberDto.AttestationDate.Value;
            }
            AttestationGrade = EnumParser.ConvertStringToEnum<Grade>(memberDto.Grade);
            GradeConfirmationStatus = memberDto.GradeConfirmationStatus;
            ResultStatus = EnumParser.ConvertStringToEnum<SeminarMemberStatus>(memberDto.ResultStatus);
        }
    }
}