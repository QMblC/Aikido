using Aikido.AdditionalData;
using Aikido.AdditionalData.Enums;
using Aikido.Dto;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMember;
using Aikido.Entities.Seminar.SeminarMemberRequest;
using System.ComponentModel.DataAnnotations;

namespace Aikido.Entities
{
    public class PaymentEntity : IDbEntity
    {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }
        public virtual UserEntity? User { get; set; }

        public long? EventId { get; set; }
        public EventType? EventType { get; set; }

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
            ISeminarMemberData member,
            decimal amount,
            PaymentType type)
        {
            UserId = member.UserId;
            Type = type;
            EventId = member.Seminar?.Id;
            EventType = AdditionalData.Enums.EventType.Seminar;
            Date = member.Seminar.Date;
            Amount = amount;
            Status = PaymentStatus.Pending;
        }

        public PaymentEntity(
            SeminarEntity seminar,
            ISeminarMemberCreation member,
            decimal amount,
            PaymentType type,
            PaymentStatus status = PaymentStatus.Pending)
        {
            UserId = member.UserId;
            Type = type;
            EventId = seminar.Id;
            EventType = AdditionalData.Enums.EventType.Seminar;
            Date = seminar.Date;
            Amount = amount;
            Status = status;
        }

        public PaymentEntity(
            SeminarEntity seminar,
            UserEntity user,
            decimal amount,
            PaymentType type,
            PaymentStatus status = PaymentStatus.Pending)
        {
            UserId = user.Id;
            Type = type;
            EventId = seminar.Id;
            EventType = AdditionalData.Enums.EventType.Seminar;
            Date = seminar.Date;
            Amount = amount;
            Status = status;
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