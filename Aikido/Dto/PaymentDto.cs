using Aikido.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto
{
    public class PaymentDto : DtoBase
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public long? ProcessedBy { get; set; }
        public string? ProcessedByName { get; set; }
        public string? Notes { get; set; }

        public PaymentDto() { }

        public PaymentDto(PaymentEntity payment)
        {
            Id = payment.Id;
            UserId = payment.UserId;
            Amount = payment.Amount;
            PaymentDate = payment.PaymentDate;
            PaymentType = payment.PaymentType.ToString();
            Description = payment.Description;
            Status = payment.Status?.ToString() ?? string.Empty;
            PaymentMethod = payment.PaymentMethod;
            TransactionId = payment.TransactionId;
            CreatedDate = payment.CreatedDate;
            ProcessedDate = payment.ProcessedDate;
            ProcessedBy = payment.ProcessedBy;
            Notes = payment.Notes;
        }
    }
}
