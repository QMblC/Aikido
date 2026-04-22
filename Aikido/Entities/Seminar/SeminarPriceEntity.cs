using Aikido.AdditionalData.Enums;
using Aikido.Dto.Seminars.Creation;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities.Seminar
{
    public class SeminarPriceEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long SeminarId {  get; set; }
        public virtual SeminarEntity Seminar { get; set; }

        public PaymentType PaymentType { get; set; }
        public Grade? CertificationGrade { get; set; }

        public decimal? Amount { get; set; }

        public SeminarPriceEntity()
        {

        }

        public SeminarPriceEntity(long seminarId, SeminarPriceCreationDto price)
        {
            SeminarId = seminarId;
            PaymentType = EnumParser.ConvertStringToEnum<PaymentType>(price.PaymentType);
            Amount = price.Amount;
            if (PaymentType == PaymentType.Certification)
            {
                CertificationGrade = EnumParser.ConvertStringToEnum<Grade>(price.CertificationGrade);
            }
        }

    }
}
