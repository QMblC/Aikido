using Aikido.Entities;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Dto
{
    public class PaymentDto : DtoBase
    {
        public long UserId { get; set; }
        public string? UserName { get; set; }
        [Required]
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public PaymentDto() { }

        public PaymentDto(PaymentEntity payment)
        {
            Id = payment.Id;
            UserId = payment.UserId;
            Amount = payment.Amount;
            PaymentDate = payment.Date;
            PaymentType = payment.Type.ToString();
            Status = payment.Status?.ToString() ?? string.Empty;
        }
    }
}
