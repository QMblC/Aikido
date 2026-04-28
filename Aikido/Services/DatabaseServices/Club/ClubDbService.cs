using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Entities.Clubs;
using Aikido.Entities.Users;
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
                .Include(c => c.Manager)             
                .Include(c => c.UserMemberships)
                    .ThenInclude(um => um.User)
                .Include(c => c.Groups)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null)
            {
                throw new EntityNotFoundException($"Клуб с Id = {id} не найден");
            }
            return club;
        }

        public async Task<List<ClubEntity>> GetManagerClubs(long managerId)
        {
            var clubs = await _context.Clubs
                .AsQueryable()
                .Where(c => c.ManagerId == managerId)
                .ToListAsync();

            return clubs;
        }

        public async Task<ClubEntity> GetClubById(long id)
        {
            return await GetByIdOrThrowException(id);
        }

        public async Task<bool> ExistsActive(long id)
        {
            return await _context.Clubs.AnyAsync(c => c.Id == id
                && c.ClosedAt == null);
        }

        public async Task<List<ClubEntity>> GetAllActiveAsync()
        {
            return await _context.Clubs
                .Include(c => c.Manager)
                .Where(c => c.ClosedAt == null)
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

        public async Task CloseAsync(long id)
        {
            var club = await GetByIdOrThrowException(id);
            club.ClosedAt = DateTime.UtcNow;
            _context.Clubs.Update(club);
            await _context.SaveChangesAsync();
        }

        public async Task RecoverAsync(long id)
        {
            var club = await GetByIdOrThrowException(id);
            club.ClosedAt = null;
            _context.Clubs.Update(club);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var club = await GetByIdOrThrowException(id);
            if (club.Groups.Count > 0)
            {
                throw new Exception("Клуб не может содержать групп");
            }
            _context.Remove(club); 
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserMembershipEntity>> GetClubMembersAsync(long clubId)
        {
            return await _context.UserMemberships
                .Include(um => um.User)
                .Include(um => um.Group)
                .Where(um => um.ClubId == clubId 
                    && um.User != null 
                    && um.Group.ClosedAt == null
                    && um.ClosedAt == null)
                .OrderBy(um => um.User.LastName)
                .ThenBy(um => um.User.FirstName)
                .ThenBy(um => um.User.MiddleName)
                .ToListAsync();
        }

        public async Task RemoveAllMembersFromClubAsync(long clubId)
        {
            var members = await _context.UserMemberships
                .Where(uc => uc.ClubId == clubId)
                .ToListAsync();

            foreach (var member in members)
            {
                _context.Remove(member);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetClubActiveMemberCountAsync(long clubId)
        {
            return await _context.UserMemberships
                .CountAsync(um => um.ClubId == clubId
                    && um.ClosedAt == null);
        }

        public async Task<List<GroupEntity>> GetClubGroupsAsync(long clubId)
        {
            return await _context.Groups
                .Include(g => g.UserMemberships
                    .Where(um => um.ClosedAt == null))
                    .ThenInclude(um => um.User)
                .Include(g => g.Schedule)
                .Include(g => g.ExclusionDates)
                .Where(g => g.ClubId == clubId && g.ClosedAt == null)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
    }
}