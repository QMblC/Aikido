using Aikido.AdditionalData.Enums;
using Aikido.Entities;

namespace Aikido.AdditionalData
{
    public interface IEvent
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public EventType EventType { get; set; }

        public ICollection<PaymentEntity> Payments { get; set; }

    }
}