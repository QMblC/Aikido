using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Dto.Seminars.Members;
using Aikido.Entities;
using Aikido.Entities.Seminar;
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
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<List<PaymentEntity>> GetPaymentsByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Payment
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<bool> IsUserPayedAnnaulFee(long userId, int year)
        {
            return _context.Payment
                .Where(p => p.Type == PaymentType.AnnualFee
                && p.Date.Date.Year == year)
                .FirstOrDefault() != null;
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

        public async Task CreateSeminarMemberPayments(
            SeminarMemberEntity member,
            SeminarMemberCreationDto memberData)
        {
            var paymentsToRemove = _context.Payment.Where(p =>
                p.SeminarMemberId == member.Id &&
                (p.Type == PaymentType.Seminar ||
                p.Type == PaymentType.BudoPassport ||
                p.Type == PaymentType.AnnualFee ||
                p.Type == PaymentType.Certification));

            _context.Payment.RemoveRange(paymentsToRemove);

            var payments = new List<PaymentEntity>();

            if (memberData.SeminarPriceInRubles != null)
            {
                var seminarPayment = new PaymentEntity(
                    member,
                    memberData.SeminarPriceInRubles.Value,
                    PaymentType.Seminar,
                    memberData.IsSeminarPayed ? PaymentStatus.Completed : PaymentStatus.Pending);
                payments.Add(seminarPayment);
            }

            if (!member.User.HasBudoPassport && memberData.BudoPassportPriceInRubles != null)
            {
                var budoPassportPayment = new PaymentEntity(
                    member,
                    memberData.BudoPassportPriceInRubles.Value,
                    PaymentType.BudoPassport,
                    memberData.IsBudoPassportPayed ? PaymentStatus.Completed : PaymentStatus.Pending);
                payments.Add(budoPassportPayment);
            }

            if (memberData.AnnualFeePriceInRubles != null && !await IsUserPayedAnnaulFee(member.UserId, member.Seminar.Date.Year))
            {
                var annualFeePayment = new PaymentEntity(
                    member,
                    memberData.AnnualFeePriceInRubles.Value,
                    PaymentType.AnnualFee,
                    memberData.IsAnnualFeePayed ? PaymentStatus.Completed : PaymentStatus.Pending);
                payments.Add(annualFeePayment);
            }

            if (memberData.CertificationPriceInRubles != null)
            {
                var certificationPayment = new PaymentEntity(
                    member,
                    memberData.CertificationPriceInRubles.Value,
                    PaymentType.Certification,
                    memberData.IsCertificationPayed ? PaymentStatus.Completed : PaymentStatus.Pending);
                payments.Add(certificationPayment);
            }

            foreach (var payment in payments)
            {
                _context.Payment.Add(payment);
            }

            await _context.SaveChangesAsync();

            member.SeminarPaymentId = payments.FirstOrDefault(p => p.Type == PaymentType.Seminar)?.Id;
            member.BudoPassportPaymentId = payments.FirstOrDefault(p => p.Type == PaymentType.BudoPassport)?.Id;
            member.AnnualFeePaymentId = payments.FirstOrDefault(p => p.Type == PaymentType.AnnualFee)?.Id;
            member.CertificationPaymentId = payments.FirstOrDefault(p => p.Type == PaymentType.Certification)?.Id;

            _context.SeminarMembers.Update(member);
            await _context.SaveChangesAsync();
        }

    }
}