using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto.Seminars;
using Aikido.Entities.Seminar;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.Seminar
{
    public class SeminarDbService : ISeminarDbService
    {
        private readonly AppDbContext _context;

        public SeminarDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SeminarEntity> GetByIdOrThrowException(long id)
        {
            var seminar = await _context.Seminars
                .Include(s => s.Instructor)
                .Include(s => s.SeminarMembers)
                    .ThenInclude(sm => sm.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seminar == null)
            {
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");
            }
            return seminar;
        }

        public async Task<bool> Exists(long id)
        {
            return await _context.Seminars.AnyAsync(s => s.Id == id);
        }

        public async Task<List<SeminarEntity>> GetAllAsync()
        {
            return await _context.Seminars
                .Include(s => s.Instructor)
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<List<SeminarMemberEntity>> GetSeminarMembersAsync(long seminarId)
        {
            return await _context.SeminarMembers
                .Include(sm => sm.User)
                .Include(sm => sm.Seminar)
                .Where(sm => sm.SeminarId == seminarId)
                .OrderBy(sm => sm.User!.FullName)
                .ToListAsync();
        }

        public async Task<long> CreateAsync(SeminarDto seminarData)
        {
            var seminar = new SeminarEntity(seminarData);
            _context.Seminars.Add(seminar);
            await _context.SaveChangesAsync();
            return seminar.Id;
        }

        public async Task UpdateAsync(long id, SeminarDto seminarData)
        {
            var seminar = await GetByIdOrThrowException(id);
            seminar.UpdateFromJson(seminarData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var seminar = await GetByIdOrThrowException(id);
            seminar.IsActive = false; // Мягкое удаление
            await _context.SaveChangesAsync();
        }

        public async Task AddMemberAsync(long seminarId, long userId)
        {
            var existingMember = await _context.SeminarMembers
                .FirstOrDefaultAsync(sm => sm.SeminarId == seminarId && sm.UserId == userId);

            if (existingMember == null)
            {
                var member = new SeminarMemberEntity
                {
                    SeminarId = seminarId,
                    UserId = userId,
                    RegistrationDate = DateTime.UtcNow,
                    Status = SeminarMemberStatus.None
                };

                _context.SeminarMembers.Add(member);
                await _context.SaveChangesAsync();

                // Обновить количество участников
                var seminar = await GetByIdOrThrowException(seminarId);
                seminar.CurrentParticipants++;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveMemberAsync(long seminarId, long userId)
        {
            var member = await _context.SeminarMembers
                .FirstOrDefaultAsync(sm => sm.SeminarId == seminarId && sm.UserId == userId);

            if (member != null)
            {
                _context.SeminarMembers.Remove(member);
                await _context.SaveChangesAsync();

                // Обновить количество участников
                var seminar = await GetByIdOrThrowException(seminarId);
                if (seminar.CurrentParticipants > 0)
                {
                    seminar.CurrentParticipants--;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<bool> IsMemberAsync(long seminarId, long userId)
        {
            return await _context.SeminarMembers
                .AnyAsync(sm => sm.SeminarId == seminarId && sm.UserId == userId);
        }

        public async Task<int> GetMemberCountAsync(long seminarId)
        {
            return await _context.SeminarMembers
                .CountAsync(sm => sm.SeminarId == seminarId);
        }

        public async Task<List<SeminarEntity>> GetSeminarsByInstructorAsync(long instructorId)
        {
            return await _context.Seminars
                .Include(s => s.Instructor)
                .Where(s => s.InstructorId == instructorId && s.IsActive)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<List<SeminarEntity>> GetSeminarsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Seminars
                .Include(s => s.Instructor)
                .Where(s => s.StartDate >= startDate && s.EndDate <= endDate && s.IsActive)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }
    }
}
