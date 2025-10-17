using Aikido.AdditionalData;
using Aikido.Dto.Seminars;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarMemberEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId { get; set; }
        public virtual SeminarEntity? Seminar { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long? GroupId { get; set; }
        public virtual SeminarGroupEntity? Group { get; set; }

        public SeminarMemberStatus Status { get; set; } = SeminarMemberStatus.None;
        public Grade OldGrade { get; set; }
        public Grade? CertificationGrade { get; set; }

        public SeminarMemberEntity() { }
        public SeminarMemberEntity(long seminarId, UserEntity user, SeminarMemberDto seminarMember)
        {
            SeminarId = seminarId;
            UserId = seminarMember.UserId;
            GroupId = seminarMember.SeminarGroupId;
            Status = EnumParser.ConvertStringToEnum<SeminarMemberStatus>(seminarMember.Status);
            OldGrade = user.Grade;
            CertificationGrade = seminarMember.CertificationGrade != null 
                ? EnumParser.ConvertStringToEnum<Grade>(seminarMember.CertificationGrade) : null;
        }
    }
}