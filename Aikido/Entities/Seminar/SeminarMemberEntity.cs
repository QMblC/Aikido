using Aikido.AdditionalData;
using Aikido.Dto.Seminars;
using Aikido.Dto.Seminars.Creation;
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

        public virtual PaymentEntity? SeminarPayment { get; set; }
        public virtual PaymentEntity? AnnualFeePayment { get; set; }
        public virtual PaymentEntity? BudoPassportPayment { get; set; }
        public virtual PaymentEntity? CertificationPayment { get; set; }
        public decimal? SeminarPriceInRubles { get; set; }
        public decimal? AnnualFeePriceInRubles { get; set; }
        public decimal? BudoPassportPriceInRubles { get; set; }
        public decimal? CertificationPriceInRubles { get; set; }

        public long? CreatorId { get; set; }
        public virtual UserEntity? Creator { get; set; }

        public SeminarMemberEntity() { }
        public SeminarMemberEntity(long coachId, SeminarEntity seminar, UserEntity user, SeminarMemberCreationDto seminarMember)
        {
            UpdateData(coachId, seminar, user, seminarMember);
        }

        public void UpdateData(long coachId, SeminarEntity seminar, UserEntity user, SeminarMemberCreationDto seminarMember)
        {
            SeminarId = seminar.Id;
            UserId = seminarMember.UserId;
            GroupId = seminarMember.SeminarGroupId;
            Status = SeminarMemberStatus.None;
            OldGrade = user.Grade;
            CertificationGrade = seminarMember.CertificationGrade != null
                ? EnumParser.ConvertStringToEnum<Grade>(seminarMember.CertificationGrade) : Grade.None; 

            SeminarPriceInRubles = seminar.SeminarPriceInRubles;
            AnnualFeePriceInRubles = seminar.AnnualFeePriceInRubles;//ToDo сделать проверку на оплату в течение года
            BudoPassportPriceInRubles = user.HasBudoPassport ? 0 : seminar.BudoPassportPriceInRubles;

            SetCertificationPrice(seminar);

            CreatorId = coachId;
        }

        private void SetCertificationPrice(SeminarEntity seminar)
        {
            if (CertificationGrade == Grade.None || CertificationGrade == null
                || CertificationGrade.ToString().ToArray()[^1] == OldGrade.ToString().ToArray()[^1])
            {
                CertificationPriceInRubles = 0;
            }
            else if ((CertificationGrade <= Grade.Kyu2 && CertificationGrade >= Grade.Kyu5)
                || CertificationGrade <= Grade.Kyu2Child && CertificationGrade >= Grade.Kyu5)
            {
                CertificationPriceInRubles = seminar.Certification5to2KyuPriceInRubles;
            }
            else if (CertificationGrade == Grade.Kyu1 || CertificationGrade == Grade.Kyu1Child)
            {
                CertificationPriceInRubles = seminar.Certification1KyuPriceInRubles;
            }
            else if (CertificationGrade >= Grade.Dan1)
            {
                CertificationPriceInRubles = seminar.CertificationDanPriceInRubles;
            }
            else
            {
                throw new NotImplementedException("Неустановлена стоимость аттестации");
            }
        }
    }
}