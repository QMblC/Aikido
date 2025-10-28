using Aikido.AdditionalData;
using Aikido.Dto;
using Aikido.Entities.Seminar;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class PaymentEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public PaymentType Type { get; set; }
        public PaymentStatus? Status { get; set; }

        public PaymentEntity() { }

        public PaymentEntity(PaymentDto paymentData)
        {
            UpdateFromJson(paymentData);
        }

        public PaymentEntity(
            SeminarMemberEntity member,
            PaymentType type,
            PaymentStatus status = PaymentStatus.Pending)
        {
            UserId = member.UserId;
            Type = type;
            Status = status;
            Date = member.Seminar.Date;
            Amount = Amount;
        }

        public void UpdateFromJson(PaymentDto paymentData)
        {
            UserId = paymentData.UserId;
            Amount = paymentData.Amount;
            Date = paymentData.PaymentDate;
            Type = EnumParser.ConvertStringToEnum<PaymentType>(paymentData.PaymentType);
            Status = EnumParser.ConvertStringToEnum<PaymentStatus>(paymentData.Status);
        }
    }
}