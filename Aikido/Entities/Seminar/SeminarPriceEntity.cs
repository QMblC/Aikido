using Aikido.AdditionalData.Enums;
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
        public CertificationPrice? CertificationPrice { get; set; }
    }
}
