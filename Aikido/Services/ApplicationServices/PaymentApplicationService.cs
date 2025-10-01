using Aikido.Dto;
using Aikido.Services.DatabaseServices;
using Aikido.Exceptions;
using Aikido.Services;

namespace Aikido.Application.Services
{
    public class PaymentApplicationService
    {
        private readonly PaymentService _paymentService;

        public PaymentApplicationService(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<PaymentDto> GetPaymentByIdAsync(long id)
        {
            var payment = await _paymentService.GetPaymentById(id);
            return new PaymentDto(payment);
        }

        public async Task<List<PaymentDto>> GetPaymentsByUserAsync(long userId)
        {
            var payments = await _paymentService.GetPaymentsByUser(userId);
            return payments.Select(p => new PaymentDto(p)).ToList();
        }

        public async Task<List<PaymentDto>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _paymentService.GetPaymentsByDateRange(startDate, endDate);
            return payments.Select(p => new PaymentDto(p)).ToList();
        }

        public async Task<long> CreatePaymentAsync(PaymentDto paymentData)
        {
            return await _paymentService.CreatePayment(paymentData);
        }

        public async Task UpdatePaymentAsync(long id, PaymentDto paymentData)
        {
            if (!await _paymentService.PaymentExists(id))
            {
                throw new EntityNotFoundException($"Платеж с Id = {id} не найден");
            }
            await _paymentService.UpdatePayment(id, paymentData);
        }

        public async Task DeletePaymentAsync(long id)
        {
            if (!await _paymentService.PaymentExists(id))
            {
                throw new EntityNotFoundException($"Платеж с Id = {id} не найден");
            }
            await _paymentService.DeletePayment(id);
        }

        public async Task<bool> PaymentExistsAsync(long id)
        {
            return await _paymentService.PaymentExists(id);
        }
    }
}
