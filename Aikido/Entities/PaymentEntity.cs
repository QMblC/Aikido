using Aikido.AdditionalData;
using Aikido.Dto;

namespace Aikido.Entities
{
    public class PaymentEntity : IDbEntity
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime Date { get; set; }
        public PaymentType Type { get; set; }
        public long Amount { get; set; } = 0;

        public PaymentEntity() { }

        public PaymentEntity(PaymentDto paymentDto)
        {
            if (paymentDto.UserId != null)
            {
                Id = paymentDto.UserId.Value;
            }
            if (paymentDto.Date != null)
            {
                Date = paymentDto.Date.Value;
            } 
            if (paymentDto.Type != null)
            {
                Type = EnumParser.ConvertStringToEnum<PaymentType>(paymentDto.Type);
            }
            Amount = paymentDto.Amount.Value;
        }
    }
}
