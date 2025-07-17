using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class PaymentService : DbService
    {
        public PaymentService(AppDbContext context) : base(context)
        {
        }

        public async Task<PaymentEntity> GetPayment(long id)
        {
            var payment = await context.Payment.FindAsync(id);
            if (payment == null)
            {
                throw new KeyNotFoundException($"Оплата с Id = {id} не найдена");
            }

            return payment;
        }

        public async Task<List<PaymentEntity>> GetPayments(long userId)
        {
            var payments = await context.Payment
                .Where(payment => payment.UserId == userId)
                .ToListAsync();

            return payments;
        }

        public async Task<List<PaymentEntity>> GetPayments(long userId, DateTime date)
        {
            var payments = await context.Payment
                .Where(payment => payment.UserId == userId)
                .Where(payment => payment.Date == date)
                .ToListAsync();

            return payments;
        }

        public async Task CreatePayment(PaymentDto paymentDto)
        {
            var paymentEntity = new PaymentEntity(paymentDto);

            context.Add(paymentEntity);

            await SaveDb();
        }

        public async Task DeletePayment(long id)
        {
            var payment = GetPayment(id);

            context.Remove(payment);

            await SaveDb();
        }

    }
}
