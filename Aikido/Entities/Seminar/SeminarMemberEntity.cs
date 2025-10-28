using Aikido.AdditionalData;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Members;
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

        public virtual ICollection<PaymentEntity> AllPayments { get; set; } = new List<PaymentEntity>();

        public long? SeminarPaymentId { get; set; }
        public virtual PaymentEntity? SeminarPayment { get; set; }

        public long? AnnualFeePaymentId { get; set; }
        public virtual PaymentEntity? AnnualFeePayment { get; set; }

        public long? BudoPassportPaymentId { get; set; }
        public virtual PaymentEntity? BudoPassportPayment { get; set; }

        public long? CertificationPaymentId { get; set; }
        public virtual PaymentEntity? CertificationPayment { get; set; }


        public long? CreatorId { get; set; }
        public virtual UserEntity? Creator { get; set; }

        public SeminarMemberEntity() { }

        public SeminarMemberEntity(
            long coachId,
            SeminarEntity seminar,
            UserEntity user, 
            SeminarMemberCreationDto seminarMember,
            SeminarMemberStatus status = SeminarMemberStatus.None)
        {
            UpdateData(coachId, seminar, user, seminarMember);
        }

        public void UpdateData(
            long coachId,
            SeminarEntity seminar,
            UserEntity user,
            SeminarMemberCreationDto seminarMember,
            SeminarMemberStatus status = SeminarMemberStatus.None)
        {
            SeminarId = seminar.Id;
            UserId = seminarMember.UserId;
            GroupId = seminarMember.SeminarGroupId;
            Status = status;
            OldGrade = user.Grade;
            CertificationGrade = seminarMember.CertificationGrade != null
                ? EnumParser.ConvertStringToEnum<Grade>(seminarMember.CertificationGrade) : Grade.None; 

            CreatorId = coachId;
        }
    }
}