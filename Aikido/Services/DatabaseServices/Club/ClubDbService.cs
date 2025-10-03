using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
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
                .Include(c => c.Manager)
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
            if (club.Groups.Count > 0)
            {
                throw new Exception("Club can\'t contain any groups before deleting");
            }
            _context.Remove(club); 
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserMembershipEntity>> GetClubMembersAsync(long clubId)
        {
            return await _context.UserMemberships
                .Include(um => um.User)
                .Where(um => um.ClubId == clubId && um.User != null)
                .OrderBy(um => um.User.LastName)
                .ThenBy(um => um.User.FirstName)
                .ThenBy(um => um.User.SecondName)
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

        public async Task<int> GetClubMemberCountAsync(long clubId)
        {
            return await _context.UserMemberships
                .CountAsync(uc => uc.ClubId == clubId);
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