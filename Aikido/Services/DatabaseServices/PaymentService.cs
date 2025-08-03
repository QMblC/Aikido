using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Dto.Seminars;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices
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

        public async Task<List<PaymentEntity>> GetCurrentYearPayment(long userId, DateTime date)
        {
            var payements = await context.Payment
                .Where(payment => payment.UserId == userId)
                .Where(payment => payment.Date.Year == date.Year)
                .ToListAsync();

            return payements;
        }

        public async Task<bool> IsUserPayedAnnualFee(long userId, DateTime date)
        {
            var payments = await GetCurrentYearPayment(userId, date);

            return payments
                .Where(payment => payment.Type == PaymentType.AnnualFee)
                .ToList()
                .FirstOrDefault() != null;
        }

        public async Task CreatePayment(PaymentDto paymentDto)
        {
            var paymentEntity = new PaymentEntity(paymentDto);

            context.Add(paymentEntity);

            await SaveChangesAsync();
        }

        public async Task CreatePayment(SeminarMemberDto member, SeminarEntity seminar)
        {
            if (member.BudoPassportPrice > 0)
            {
                var payment = new PaymentEntity()
                {
                    UserId = member.Id.Value,
                    Date = seminar.Date,
                    Type = PaymentType.BudoPassport,
                    Amount = (long)member.BudoPassportPrice
                };
                context.Add(payment);
                await SaveChangesAsync();
            }
            if (member.AnnualFee > 0)
            {
                var payment = new PaymentEntity()
                {
                    UserId = member.Id.Value,
                    Date = seminar.Date,
                    Type = PaymentType.AnnualFee,
                    Amount = (long)member.AnnualFee
                };
                context.Add(payment);
                await SaveChangesAsync();
            }
            if (member.SeminarPrice > 0)
            {
                var payment = new PaymentEntity()
                {
                    UserId = member.Id.Value,
                    Date = seminar.Date,
                    Type = PaymentType.Seminar,
                    Amount = (long)member.SeminarPrice
                };
                context.Add(payment);
                await SaveChangesAsync();
            }
            if (member.CertificationPrice > 0)
            {
                var payment = new PaymentEntity()
                {
                    UserId = member.Id.Value,
                    Date = seminar.Date,
                    Type = PaymentType.Certification,
                    Amount = (long)member.CertificationPrice
                };
                context.Add(payment);
                await SaveChangesAsync();
            }
        }

        public async Task DeletePayment(SeminarMemberDto member, SeminarEntity seminar)
        {
            var payements = context.Payment.Where(payement => payement.UserId == member.Id 
                && payement.Date == seminar.Date);

            foreach (var payement in payements)
            {
                if (payement.Type == PaymentType.Certification 
                    || payement.Type == PaymentType.AnnualFee
                    || payement.Type == PaymentType.Seminar
                    || payement.Type == PaymentType.BudoPassport)
                {
                    context.Payment.Remove(payement);
                }
                       
            }

            await SaveChangesAsync();
        }

        public async Task DeletePayment(long id)
        {
            var payment = GetPayment(id);

            context.Remove(payment);

            await SaveChangesAsync();
        }

    }
}
