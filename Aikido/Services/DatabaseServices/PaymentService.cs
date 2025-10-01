using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class PaymentService
    {
        private readonly AppDbContext _context;

        public PaymentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentEntity> GetPaymentById(long id)
        {
            var payment = await _context.Payment.FindAsync(id);
            if (payment == null)
                throw new EntityNotFoundException($"Платеж с Id = {id} не найден");
            return payment;
        }

        public async Task<List<PaymentEntity>> GetPaymentsByUser(long userId)
        {
            return await _context.Payment
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<List<PaymentEntity>> GetPaymentsByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Payment
                .Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<long> CreatePayment(PaymentDto paymentData)
        {
            var payment = new PaymentEntity(paymentData);
            _context.Payment.Add(payment);
            await _context.SaveChangesAsync();
            return payment.Id;
        }

        public async Task UpdatePayment(long id, PaymentDto paymentData)
        {
            var payment = await GetPaymentById(id);
            payment.UpdateFromJson(paymentData);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePayment(long id)
        {
            var payment = await GetPaymentById(id);
            _context.Payment.Remove(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PaymentExists(long id)
        {
            return await _context.Payment.AnyAsync(p => p.Id == id);
        }
    }
}
