using Aikido.AdditionalData.Enums;
using Aikido.Dto;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Exceptions;

namespace Aikido.Services.DatabaseServices.Payment
{
    public interface IPaymentDbService
    {
        Task<PaymentEntity> GetPaymentById(long id);

        Task<List<PaymentEntity>> GetPaymentsByUser(long userId);

        Task<List<PaymentEntity>> GetSeminarMemberPayments(long seminarId, long userId);

        Task<List<PaymentEntity>> GetFakeSeminarMemberPayment(long seminarId, long userId);
        Task<List<PaymentEntity>> GetPaymentsByDateRange(DateTime startDate, DateTime endDate);

        Task<bool> IsUserPayedAnnaulFee(long userId, int year);

        Task<long> CreatePayment(PaymentDto paymentData);

        Task UpdatePayment(long id, PaymentDto paymentData);

        Task DeletePayment(long id);

        Task<bool> PaymentExists(long id);

        Task CreateOrUpdateMemberPayments(long seminarId, ISeminarMemberCreation memberData);
    }
}
