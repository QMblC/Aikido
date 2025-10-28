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

        public async Task CreateOrUpdateSeminarMemberPayments(
    SeminarMemberEntity member,
    SeminarMemberCreationDto memberData)
        {
            // Загружаем существующие оплаты
            await LoadExistingPayments(member);

            // Обрабатываем оплату семинара
            await ProcessSeminarPayment(member, memberData.SeminarPrice, memberData.IsSeminarPayed);

            // Обрабатываем оплату будо-паспорта
            await ProcessBudoPassportPayment(member, memberData.BudoPassportPrice, memberData.IsBudoPassportPayed);

            // Обрабатываем годовой взнос
            await ProcessAnnualFeePayment(member, memberData.AnnualFeePrice, memberData.IsAnnualFeePayed);

            // Обрабатываем оплату аттестации
            await ProcessCertificationPayment(member, memberData.CertificationPrice, memberData.IsCertificationPayed);

            // Сохраняем изменения
            _context.SeminarMembers.Update(member);
            await _context.SaveChangesAsync();
        }

        private async Task LoadExistingPayments(SeminarMemberEntity member)
        {
            if (member.SeminarPaymentId.HasValue && member.SeminarPayment == null)
            {
                member.SeminarPayment = await _context.Payment.FindAsync(member.SeminarPaymentId.Value);
            }
            if (member.BudoPassportPaymentId.HasValue && member.BudoPassportPayment == null)
            {
                member.BudoPassportPayment = await _context.Payment.FindAsync(member.BudoPassportPaymentId.Value);
            }
            if (member.AnnualFeePaymentId.HasValue && member.AnnualFeePayment == null)
            {
                member.AnnualFeePayment = await _context.Payment.FindAsync(member.AnnualFeePaymentId.Value);
            }
            if (member.CertificationPaymentId.HasValue && member.CertificationPayment == null)
            {
                member.CertificationPayment = await _context.Payment.FindAsync(member.CertificationPaymentId.Value);
            }
        }

        private async Task ProcessSeminarPayment(SeminarMemberEntity member, decimal? price, bool isPaid)
        {
            if (price == null)
            {
                if (member.SeminarPayment != null)
                {
                    _context.Payment.Remove(member.SeminarPayment);
                    member.SeminarPayment = null;
                    member.SeminarPaymentId = null;
                }
                return;
            }

            var status = isPaid ? PaymentStatus.Completed : PaymentStatus.Pending;

            if (member.SeminarPayment != null)
            {
                member.SeminarPayment.Amount = price.Value;
                member.SeminarPayment.Status = status;
                member.SeminarPayment.Date = member.Seminar.Date;
                _context.Payment.Update(member.SeminarPayment);
            }
            else
            {
                var payment = new PaymentEntity(member, price.Value, PaymentType.Seminar, status);
                _context.Payment.Add(payment);
                await _context.SaveChangesAsync(); 

                member.SeminarPayment = payment;
                member.SeminarPaymentId = payment.Id;
            }
        }

        private async Task ProcessBudoPassportPayment(SeminarMemberEntity member, decimal? price, bool isPaid)
        {
            if (member.User.HasBudoPassport)
            {
                if (member.BudoPassportPayment != null)
                {
                    _context.Payment.Remove(member.BudoPassportPayment);
                    member.BudoPassportPayment = null;
                    member.BudoPassportPaymentId = null;
                }
                return;
            }

            if (price == null)
            {
                if (member.BudoPassportPayment != null)
                {
                    _context.Payment.Remove(member.BudoPassportPayment);
                    member.BudoPassportPayment = null;
                    member.BudoPassportPaymentId = null;
                }
                return;
            }

            var status = isPaid ? PaymentStatus.Completed : PaymentStatus.Pending;

            if (member.BudoPassportPayment != null)
            {
                member.BudoPassportPayment.Amount = price.Value;
                member.BudoPassportPayment.Status = status;
                member.BudoPassportPayment.Date = member.Seminar.Date;
                _context.Payment.Update(member.BudoPassportPayment);
            }
            else
            {
                var payment = new PaymentEntity(member, price.Value, PaymentType.BudoPassport, status);
                _context.Payment.Add(payment);
                await _context.SaveChangesAsync(); 

                member.BudoPassportPayment = payment;
                member.BudoPassportPaymentId = payment.Id;
            }
        }

        private async Task ProcessAnnualFeePayment(SeminarMemberEntity member, decimal? price, bool isPaid)
        {
            if (price == null)
            {
                if (member.AnnualFeePayment != null)
                {
                    _context.Payment.Remove(member.AnnualFeePayment);
                    member.AnnualFeePayment = null;
                    member.AnnualFeePaymentId = null;
                }
                return;
            }

            var status = isPaid ? PaymentStatus.Completed : PaymentStatus.Pending;

            if (member.AnnualFeePayment != null)
            {
                member.AnnualFeePayment.Amount = price.Value;
                member.AnnualFeePayment.Status = status;
                member.AnnualFeePayment.Date = member.Seminar.Date;
                _context.Payment.Update(member.AnnualFeePayment);
            }
            else
            {
                var payment = new PaymentEntity(member, price.Value, PaymentType.AnnualFee, status);
                _context.Payment.Add(payment);
                await _context.SaveChangesAsync();

                member.AnnualFeePayment = payment;
                member.AnnualFeePaymentId = payment.Id;
            }
        }

        private async Task ProcessCertificationPayment(SeminarMemberEntity member, decimal? price, bool isPaid)
        {
            if (price == null)
            {
                if (member.CertificationPayment != null)
                {
                    _context.Payment.Remove(member.CertificationPayment);
                    member.CertificationPayment = null;
                    member.CertificationPaymentId = null;
                }
                return;
            }

            var status = isPaid ? PaymentStatus.Completed : PaymentStatus.Pending;

            if (member.CertificationPayment != null)
            {
                member.CertificationPayment.Amount = price.Value;
                member.CertificationPayment.Status = status;
                member.CertificationPayment.Date = member.Seminar.Date;
                _context.Payment.Update(member.CertificationPayment);
            }
            else
            {
                var payment = new PaymentEntity(member, price.Value, PaymentType.Certification, status);
                _context.Payment.Add(payment);
                await _context.SaveChangesAsync(); 

                member.CertificationPayment = payment;
                member.CertificationPaymentId = payment.Id;
            }
        }

    }
}