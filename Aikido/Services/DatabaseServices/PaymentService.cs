using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Dto.Seminars.Creation;
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
            var payments = new List<PaymentEntity>();

            if (memberData.SeminarPrice != null)
            {
                var seminarPayment = new PaymentEntity(
                    member,
                    memberData.SeminarPrice.Value,
                    PaymentType.Seminar,
                    memberData.IsSeminarPayed ? PaymentStatus.Completed : PaymentStatus.Pending);
                payments.Add(seminarPayment);
            }

            if (!member.User.HasBudoPassport && memberData.BudoPassportPrice != null)
            {
                var budoPassportPayment = new PaymentEntity(
                    member,
                    memberData.BudoPassportPrice.Value,
                    PaymentType.BudoPassport,
                    memberData.IsBudoPassportPayed ? PaymentStatus.Completed : PaymentStatus.Pending);
                payments.Add(budoPassportPayment);
            }

            if (memberData.AnnualFeePrice != null)
            {
                var annualFeePayment = new PaymentEntity(
                    member,
                    memberData.AnnualFeePrice.Value,
                    PaymentType.AnnualFee,
                    memberData.IsAnnualFeePayed ? PaymentStatus.Completed : PaymentStatus.Pending);
                payments.Add(annualFeePayment);
            }

            if (memberData.CertificationPrice != null)
            {
                var certificationPayment = new PaymentEntity(
                    member,
                    memberData.CertificationPrice.Value,
                    PaymentType.Certification,
                    memberData.IsCertificationPayed ? PaymentStatus.Completed : PaymentStatus.Pending);
                payments.Add(certificationPayment);
            }

            foreach (var payment in payments)
            {
                _context.Payment.Add(payment);
            }

            await _context.SaveChangesAsync();

            if (memberData.SeminarPrice != null)
            {
                member.SeminarPaymentId = payments.First(p => p.Type == PaymentType.Seminar).Id;
            }
            if (!member.User.HasBudoPassport && memberData.BudoPassportPrice != null)
            {
                member.BudoPassportPaymentId = payments.First(p => p.Type == PaymentType.BudoPassport).Id;
            }
            if (memberData.AnnualFeePrice != null)
            {
                member.AnnualFeePaymentId = payments.First(p => p.Type == PaymentType.AnnualFee).Id;
            }
            if (memberData.CertificationPrice != null)
            {
                member.CertificationPaymentId = payments.First(p => p.Type == PaymentType.Certification).Id;
            }

            _context.SeminarMembers.Update(member);
            await _context.SaveChangesAsync();
        }
    }
}