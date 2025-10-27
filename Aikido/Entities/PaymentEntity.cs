using Aikido.AdditionalData;
using Aikido.Dto;
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
        public DateTime PaymentDate { get; set; }
        public PaymentType PaymentType { get; set; }
        public PaymentStatus? Status { get; set; }

        public PaymentEntity() { }

        public PaymentEntity(PaymentDto paymentData)
        {
            UpdateFromJson(paymentData);
        }

        public void UpdateFromJson(PaymentDto paymentData)
        {
            UserId = paymentData.UserId;
            Amount = paymentData.Amount;
            PaymentDate = paymentData.PaymentDate;
            PaymentType = EnumParser.ConvertStringToEnum<PaymentType>(paymentData.PaymentType);
            Status = EnumParser.ConvertStringToEnum<PaymentStatus>(paymentData.Status);
        }
    }

    public enum PaymentStatus
    {
        Pending,
        Completed
    }
}