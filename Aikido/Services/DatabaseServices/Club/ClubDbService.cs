using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.Club
{
    public class ClubDbService : IClubDbService
    {
        private readonly AppDbContext _context;

        public ClubDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ClubEntity> GetByIdOrThrowException(long id)
        {
            var club = await _context.Clubs
                .Include(c => c.HeadCoach)
                .Include(c => c.UserClubs)
                    .ThenInclude(uc => uc.User)
                .Include(c => c.Groups)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
            {
                throw new EntityNotFoundException($"Клуб с Id = {id} не найден");
            }
            return club;
        }

        public async Task<ClubEntity> GetClubById(long id)
        {
            return await GetByIdOrThrowException(id);
        }

        public async Task<bool> Exists(long id)
        {
            return await _context.Clubs.AnyAsync(c => c.Id == id);
        }

        public async Task<List<ClubEntity>> GetAllAsync()
        {
            return await _context.Clubs
                .Include(c => c.HeadCoach)
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<long> CreateAsync(ClubDto clubData)
        {
            var club = new ClubEntity(clubData);
            _context.Clubs.Add(club);
            await _context.SaveChangesAsync();
            return club.Id;
        }

        public async Task UpdateAsync(long id, ClubDto clubData)
        {
            var club = await GetByIdOrThrowException(id);
            club.UpdateFromJson(clubData);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var club = await GetByIdOrThrowException(id);
            club.IsActive = false; // Мягкое удаление
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserClub>> GetClubMembersAsync(long clubId)
        {
            return await _context.UserClubs
                .Include(uc => uc.User)
                .Where(uc => uc.ClubId == clubId)
                .OrderBy(uc => uc.User!.FullName)
                .ToListAsync();
        }

        public async Task RemoveAllMembersFromClubAsync(long clubId)
        {
            var members = await _context.UserClubs
                .Where(uc => uc.ClubId == clubId && uc.IsActive)
                .ToListAsync();

            foreach (var member in members)
            {
                member.IsActive = false;
                member.LeaveDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetClubMemberCountAsync(long clubId)
        {
            return await _context.UserClubs
                .CountAsync(uc => uc.ClubId == clubId && uc.IsActive);
        }

        public async Task<List<GroupEntity>> GetClubGroupsAsync(long clubId)
        {
            return await _context.Groups
                .Include(g => g.Coach)
                .Where(g => g.ClubId == clubId && g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
    }
}