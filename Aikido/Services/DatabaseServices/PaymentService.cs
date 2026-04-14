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
            var seminar = await _context.Seminars
                .Include(s => s.ManagerRequestMembers)
                .Include(s => s.Prices)
                .FirstOrDefaultAsync(s => s.Id == seminarId);

            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            var payments = new List<PaymentEntity>();

            var user = await _context.Users.FindAsync(userId);

            var isUserPayedAnnualFee = await IsUserPayedAnnaulFee(userId, seminar.Date.Year);

            if (!isUserPayedAnnualFee)
            {
                payments.Add(new PaymentEntity(seminar, user, seminar.Prices.First(p => p.PaymentType == PaymentType.AnnualFee)));
            }

            if (!user.HasBudoPassport)
            {
                payments.Add(new PaymentEntity(seminar, user, seminar.Prices.First(p => p.PaymentType == PaymentType.BudoPassport)));
            }

            payments.Add(new PaymentEntity(seminar, user, seminar.Prices.First(p => p.PaymentType == PaymentType.Seminar)));

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
            var seminar = await _context.Seminars
                .Include(s => s.ManagerRequestMembers)
                .Include(s => s.Prices)
                .FirstOrDefaultAsync(s => s.Id == seminarId);

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
                    payments.Add(new PaymentEntity(member, seminar.Prices.First(p => p.PaymentType == PaymentType.AnnualFee)));
                }

                if (!member.User.HasBudoPassport)
                {
                    payments.Add(new PaymentEntity(member, seminar.Prices.First(p => p.PaymentType == PaymentType.BudoPassport)));
                }

                if (member.CertificationGrade != Grade.None)
                {
                    payments.Add(new PaymentEntity(member, DetermineCertificationPrice(seminar, member)));
                }

                payments.Add(new PaymentEntity(member, seminar.Prices.First(p => p.PaymentType == PaymentType.Seminar)));
            }
            
            await _context.AddRangeAsync(payments);
            await _context.SaveChangesAsync();
        }

        private SeminarPriceEntity DetermineCertificationPrice(SeminarEntity seminar, ISeminarMemberData member)
        {
            var paymentType = member.CertificationGrade switch
            {
                Grade.Kyu5Child => CertificationPaymentType.Certification5KyuChild,
                Grade.Kyu4Child => CertificationPaymentType.Certification4KyuChild,
                Grade.Kyu3Child => CertificationPaymentType.Certification3KyuChild,
                Grade.Kyu2Child => CertificationPaymentType.Certification2KyuChild,
                Grade.Kyu1Child => CertificationPaymentType.Certification1KyuChild,

                Grade.Kyu5 => CertificationPaymentType.Certification5Kyu,
                Grade.Kyu4 => CertificationPaymentType.Certification4Kyu,
                Grade.Kyu3 => CertificationPaymentType.Certification3Kyu,
                Grade.Kyu2 => CertificationPaymentType.Certification2Kyu,
                Grade.Kyu1 => CertificationPaymentType.Certification1Kyu,

                Grade.Dan1 => CertificationPaymentType.Certification1Dan,
                Grade.Dan2 => CertificationPaymentType.Certification2Dan,
                Grade.Dan3 => CertificationPaymentType.Certification3Dan,
                Grade.Dan4 => CertificationPaymentType.Certification4Dan,
                Grade.Dan5 => CertificationPaymentType.Certification5Dan,
                Grade.Dan6 => CertificationPaymentType.Certification6Dan,
                Grade.Dan7 => CertificationPaymentType.Certification7Dan,
                Grade.Dan8 => CertificationPaymentType.Certification8Dan,
                Grade.Dan9 => CertificationPaymentType.Certification9Dan,
                Grade.Dan10 => CertificationPaymentType.Certification10Dan,

                _ => throw new ArgumentOutOfRangeException(nameof(member.CertificationGrade))
            };

            return seminar.Prices.First(p => p.CertificationPaymentType == paymentType);
        }

        public async Task CreateOrUpdateMemberPayments(long seminarId, ISeminarMemberCreation memberData)
        {
            var seminar = await _context.Seminars.Include(s => s.ManagerRequestMembers).FirstAsync(s => s.Id == seminarId)
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
    }
}