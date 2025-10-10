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
                .Include(s => s.Creator)
                .Include(s => s.Groups)
                .Include(s => s.ContactInfo)
                .Include(s => s.Schedule)
                .Include(s => s.Regulation)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seminar == null)
                throw new EntityNotFoundException($"Семинар с Id = {id} не найден");

            return seminar;
        }

        public async Task<bool> Exists(long id)
        {
            return await _context.Seminars.AnyAsync(s => s.Id == id);
        }

        public async Task<List<SeminarEntity>> GetAllAsync()
        {
            return await _context.Seminars
                .Include(s => s.Creator)
                .Include(s => s.Groups)
                .Include(s => s.ContactInfo)
                .Include(s => s.Schedule)
                .Include(s => s.Regulation)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
        }

        public async Task<List<SeminarMemberEntity>> GetSeminarMembersAsync(long seminarId)
        {
            return await _context.SeminarMembers
                .Include(sm => sm.User)
                .Include(sm => sm.Seminar)
                .Include(sm => sm.Group)
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
            _context.Remove(seminar);
            await _context.SaveChangesAsync();
        }

        public async Task AddSeminarMembersAsync(long seminarId, List<SeminarMemberDto> membersDto)
        {
            foreach (var memberDto in membersDto)
            {
                var exists = await _context.SeminarMembers
                    .AnyAsync(sm => sm.SeminarId == seminarId && sm.UserId == memberDto.UserId);

                if (!exists)
                {
                    var member = new SeminarMemberEntity(seminarId, memberDto);
                    _context.SeminarMembers.Add(member);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Seminar member already exists");
                }
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
            }
        }

        public async Task<bool> IsMemberAsync(long seminarId, long userId)
        {
            return await _context.SeminarMembers
                .AnyAsync(sm => sm.SeminarId == seminarId && sm.UserId == userId);
        }

        public async Task<int> GetMemberCountAsync(long seminarId)
        {
            return await _context.SeminarMembers.CountAsync(sm => sm.SeminarId == seminarId);
        }

        public async Task<List<SeminarEntity>> GetSeminarsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Seminars
                .Include(s => s.Creator)
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<SeminarRegulationEntity> GetSeminarRegulation(long seminarId)
        {
            var regulation = await _context.SeminarRegulation.FirstOrDefaultAsync(r => r.SeminarId == seminarId);

            return regulation 
                ?? throw new EntityNotFoundException(nameof(SeminarRegulationEntity));
        }

        public async Task CreateSeminarRegulationAsync(long seminarId, byte[] fileInBytes)
        {
            var seminar = await _context.Seminars.FindAsync(seminarId);
            var oldRegulation = await _context.SeminarRegulation.FirstOrDefaultAsync(r => r.SeminarId == seminarId);
            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(seminar));
            }

            if (oldRegulation != null)
            {
                _context.Remove(oldRegulation);
            }

            var regulation = new SeminarRegulationEntity(seminarId, fileInBytes);
            _context.SeminarRegulation.Add(regulation);
            seminar.RegulationId = regulation.Id;

            await _context.SaveChangesAsync();
        }


        public async Task DeleteSeminarRegulationAsync(long seminarId)
        {
            var seminar = _context.Seminars.Find(seminarId);
            var regulation = _context.SeminarRegulation.FirstOrDefaultAsync(r => r.SeminarId == seminarId);

            if (seminar == null)
            {
                throw new EntityNotFoundException(nameof(SeminarEntity));
            }
            if (regulation == null)
            {
                throw new EntityNotFoundException(nameof(SeminarRegulationEntity));
            }
            
            _context.Remove(regulation);
            seminar.RegulationId = null;
            await _context.SaveChangesAsync();
        }
    }
}
