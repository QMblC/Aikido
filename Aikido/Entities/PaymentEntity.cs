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
        public string? Description { get; set; }
        public PaymentStatus? Status { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedDate { get; set; }
        public long? ProcessedBy { get; set; }
        public virtual UserEntity? ProcessedByUser { get; set; }
        public string? Notes { get; set; }

        // Дополнительные поля для связи с клубом или группой
        public long? ClubId { get; set; }
        public virtual ClubEntity? Club { get; set; }

        public long? GroupId { get; set; }
        public virtual GroupEntity? Group { get; set; }

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
            Description = paymentData.Description;
            if (!string.IsNullOrEmpty(paymentData.Status))
                Status = EnumParser.ConvertStringToEnum<PaymentStatus>(paymentData.Status);
            PaymentMethod = paymentData.PaymentMethod;
            TransactionId = paymentData.TransactionId;
            ProcessedDate = paymentData.ProcessedDate;
            ProcessedBy = paymentData.ProcessedBy;
            Notes = paymentData.Notes;
        }
    }

    public enum PaymentStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled,
        Refunded
    }
}