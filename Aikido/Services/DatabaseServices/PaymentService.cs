using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Dto.Seminars.Members.Creation;
using Aikido.Entities;
using Aikido.Entities.Seminar;
using Aikido.Entities.Seminar.SeminarMember;
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
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
                throw new EntityNotFoundException($"Платеж с Id = {id} не найден");
            return payment;
        }

        public async Task<List<PaymentEntity>> GetPaymentsByUser(long userId)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<List<PaymentEntity>> GetSeminarMemberPayments(long seminarId, long userId)
        {
            return await _context.Payments.AsQueryable()
                .Where(p => p.EventId == seminarId
                && p.EventType == EventType.Seminar
                && p.UserId == userId)
                .ToListAsync();
        }

        public async Task<List<PaymentEntity>> GetFakeSeminarMemberPayment(long seminarId, long userId)
        {
            var seminar = await _context.Seminars.Include(s => s.ManagerRequestMembers).FirstOrDefaultAsync(s => s.Id == seminarId);

            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            var payments = new List<PaymentEntity>();

            var user = await _context.Users.FindAsync(userId);

            var isUserPayedAnnualFee = await IsUserPayedAnnaulFee(userId, seminar.Date.Year);

            if (!isUserPayedAnnualFee)
            {
                payments.Add(new PaymentEntity(seminar, user, seminar.AnnualFeePriceInRubles.Value, PaymentType.AnnualFee));
            }

            if (!user.HasBudoPassport)
            {
                payments.Add(new PaymentEntity(seminar, user, seminar.BudoPassportPriceInRubles.Value, PaymentType.BudoPassport));
            }

            payments.Add(new PaymentEntity(seminar, user, seminar.SeminarPriceInRubles.Value, PaymentType.Seminar));

            return payments;
        }

        public async Task<List<PaymentEntity>> GetPaymentsByDateRange(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.Date >= startDate && p.Date <= endDate)
                .OrderByDescending(p => p.Date)
                .ToListAsync();
        }

        public async Task<bool> IsUserPayedAnnaulFee(long userId, int year)
        {
            return _context.Payments
                .Where(p => p.Type == PaymentType.AnnualFee
                && p.Date.Year == year
                && p.UserId == userId)
                .FirstOrDefault() != null;
        }

        public async Task<long> CreatePayment(PaymentDto paymentData)
        {
            var payment = new PaymentEntity(paymentData);
            _context.Payments.Add(payment);
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
            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> PaymentExists(long id)
        {
            return await _context.Payments.AnyAsync(p => p.Id == id);
        }

        public async Task CreateSeminarPayments(long seminarId)
        {
            var seminar = await _context.Seminars.Include(s => s.ManagerRequestMembers).FirstOrDefaultAsync(s => s.Id == seminarId);

            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            var payments = new List<PaymentEntity>();

            foreach (var member in seminar.ManagerRequestMembers)
            {
                var isUserPayedAnnualFee = await IsUserPayedAnnaulFee(member.UserId, seminar.Date.Year);

                if (!isUserPayedAnnualFee)
                {
                    payments.Add(new PaymentEntity(member, seminar.AnnualFeePriceInRubles.Value, PaymentType.AnnualFee));
                }

                if (!member.User.HasBudoPassport)
                {
                    payments.Add(new PaymentEntity(member, seminar.BudoPassportPriceInRubles.Value, PaymentType.BudoPassport));
                }

                if (member.CertificationGrade != Grade.None)
                {
                    payments.Add(new PaymentEntity(member, DetermineCertificationPrice(seminar, member), PaymentType.Certification));
                }

                payments.Add(new PaymentEntity(member, seminar.SeminarPriceInRubles.Value, PaymentType.Seminar));
            }
            
            await _context.AddRangeAsync(payments);
            await _context.SaveChangesAsync();
        }

        private decimal DetermineCertificationPrice(SeminarEntity seminar, ISeminarMemberData member)
        {
            if (member.CertificationGrade >= Grade.Dan1)
            {
                return seminar.CertificationDanPriceInRubles.Value;
            }
            else if (member.CertificationGrade == Grade.Kyu1 || member.CertificationGrade == Grade.Kyu1Child)
            {
                return seminar.Certification1KyuPriceInRubles.Value;
            }
            else
            {
                return seminar.Certification5to2KyuPriceInRubles.Value;
            }
        }

        public async Task CreateOrUpdateMemberPayments(long seminarId, ISeminarMemberCreation memberData)
        {
            var seminar = await _context.Seminars.FindAsync(seminarId)
                ?? throw new EntityNotFoundException(nameof(SeminarEntity));

            var seminarMemberPayments = await _context.Payments
                .Where(p => p.UserId == memberData.UserId
                    && p.EventId == seminarId
                    && p.EventType == EventType.Seminar)
                .ToListAsync();

            var paymentsToCreate = new List<PaymentEntity>();
            var paymentsToUpdate = new List<PaymentEntity>();
            var paymentsToDelete = new List<PaymentEntity>();

            void UpsertPayment(
                PaymentType type,
                decimal? price,
                bool isPayed)
            {
                if (price is null)
                {
                    var existing = seminarMemberPayments.FirstOrDefault(p => p.Type == type);
                    if (existing is not null)
                        paymentsToDelete.Add(existing);
                    return;
                }

                var payment = seminarMemberPayments.FirstOrDefault(p => p.Type == type);

                if (payment is null)
                {
                    paymentsToCreate.Add(new PaymentEntity(
                        seminar,
                        memberData,
                        price.Value,
                        type,
                        isPayed ? PaymentStatus.Completed : PaymentStatus.Pending));
                }
                else
                {
                    payment.Status = isPayed ? PaymentStatus.Completed : PaymentStatus.Pending;
                    payment.Amount = price.Value;
                    paymentsToUpdate.Add(payment);
                }
            }

            UpsertPayment(PaymentType.Seminar, memberData.SeminarPriceInRubles, memberData.IsSeminarPayed);
            UpsertPayment(PaymentType.AnnualFee, memberData.AnnualFeePriceInRubles, memberData.IsAnnualFeePayed);
            UpsertPayment(PaymentType.BudoPassport, memberData.BudoPassportPriceInRubles, memberData.IsBudoPassportPayed);
            UpsertPayment(PaymentType.Certification, memberData.CertificationPriceInRubles, memberData.IsCertificationPayed);

            if (paymentsToCreate.Count > 0)
                await _context.Payments.AddRangeAsync(paymentsToCreate);

            if (paymentsToUpdate.Count > 0)
                _context.Payments.UpdateRange(paymentsToUpdate);

            if (paymentsToDelete.Count > 0)
                _context.Payments.RemoveRange(paymentsToDelete);

            await _context.SaveChangesAsync();
        }


        //public async Task CreateSeminarMemberPayments(
        //    SeminarMemberEntity member,
        //    SeminarMemberCreationDto memberData)
        //{
        //    var paymentsToRemove = _context.Payment.Where(p =>
        //        p.SeminarMemberId == member.Id &&
        //        (p.Type == PaymentType.Seminar ||
        //        p.Type == PaymentType.BudoPassport ||
        //        p.Type == PaymentType.AnnualFee ||
        //        p.Type == PaymentType.Certification));

        //    _context.Payment.RemoveRange(paymentsToRemove);

        //    var payments = new List<PaymentEntity>();

        //    if (memberData.SeminarPriceInRubles != null)
        //    {
        //        var seminarPayment = new PaymentEntity(
        //            member,
        //            memberData.SeminarPriceInRubles.Value,
        //            PaymentType.Seminar,
        //            memberData.IsSeminarPayed ? PaymentStatus.Completed : PaymentStatus.Pending);
        //        payments.Add(seminarPayment);
        //    }

        //    if (!member.User.HasBudoPassport && memberData.BudoPassportPriceInRubles != null)
        //    {
        //        var budoPassportPayment = new PaymentEntity(
        //            member,
        //            memberData.BudoPassportPriceInRubles.Value,
        //            PaymentType.BudoPassport,
        //            memberData.IsBudoPassportPayed ? PaymentStatus.Completed : PaymentStatus.Pending);
        //        payments.Add(budoPassportPayment);
        //    }

        //    if (memberData.AnnualFeePriceInRubles != null && !await IsUserPayedAnnaulFee(member.UserId, member.Seminar.Date.Year))
        //    {
        //        var annualFeePayment = new PaymentEntity(
        //            member,
        //            memberData.AnnualFeePriceInRubles.Value,
        //            PaymentType.AnnualFee,
        //            memberData.IsAnnualFeePayed ? PaymentStatus.Completed : PaymentStatus.Pending);
        //        payments.Add(annualFeePayment);
        //    }

        //    if (memberData.CertificationPriceInRubles != null)
        //    {
        //        var certificationPayment = new PaymentEntity(
        //            member,
        //            memberData.CertificationPriceInRubles.Value,
        //            PaymentType.Certification,
        //            memberData.IsCertificationPayed ? PaymentStatus.Completed : PaymentStatus.Pending);
        //        payments.Add(certificationPayment);
        //    }

        //    foreach (var payment in payments)
        //    {
        //        _context.Payment.Add(payment);
        //    }

        //    await _context.SaveChangesAsync();

        //    member.SeminarPaymentId = payments.FirstOrDefault(p => p.Type == PaymentType.Seminar)?.Id;
        //    member.BudoPassportPaymentId = payments.FirstOrDefault(p => p.Type == PaymentType.BudoPassport)?.Id;
        //    member.AnnualFeePaymentId = payments.FirstOrDefault(p => p.Type == PaymentType.AnnualFee)?.Id;
        //    member.CertificationPaymentId = payments.FirstOrDefault(p => p.Type == PaymentType.Certification)?.Id;

        //    _context.SeminarMembers.Update(member);
        //    await _context.SaveChangesAsync();
        //}

    }
}