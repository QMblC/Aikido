using Aikido.AdditionalData;
using Aikido.Entities;

namespace Aikido.Dto
{
    public class PaymentDto : DtoBase
    {
        public long? Id { get; set; }
        public long? UserId { get; set; }
        public DateTime? Date { get; set; }
        public string? Type { get; set; }
        public long? Amount { get; set; } = 0;

        public PaymentDto() { }

        public PaymentDto(
            long? userId,
            DateTime? date,
            PaymentType type,
            long? amount)
        {
            UserId = userId;
            Date = date;
            Type = EnumParser.ConvertEnumToString(type);
            Amount = amount;
        }

        public PaymentDto(PaymentEntity paymentEntity)
        {
            Id = paymentEntity.Id;
            UserId = paymentEntity.UserId;
            Date = paymentEntity.Date;
            Type = EnumParser.ConvertEnumToString(paymentEntity.Type);
            Amount = paymentEntity.Amount;
        }
    }
}
