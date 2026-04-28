
using Aikido.Data;
using Aikido.Entities.Clubs;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.Club
{
    public class ClubStaffDbService : IClubStaffDbService
    {
        private readonly AppDbContext _context;

        public ClubStaffDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ClubStaffEntity> GetClubStaff(long clubId, long userId)
        {
            var staff = await _context.ClubStaff.FirstOrDefaultAsync(cs =>
                    cs.ClubId == clubId
                    && cs.UserId == userId);

            if (staff == null)
            {
                throw new EntityNotFoundException(nameof(ClubStaffEntity));
            }

            return staff;
        }

        public async Task<List<ClubStaffEntity>> GetClubStaffByClub(long clubId)
        {
            var clubStaff = await _context.ClubStaff
                .Include(cs => cs.User)
                .Where(cs => cs.ClubId == clubId)
                .ToListAsync();

            return clubStaff ?? new();
        }

        public async Task<List<ClubStaffEntity>> GetClubStaffByUser(long userId)
        {
            var clubStaff = await _context.ClubStaff
                .Include(cs => cs.Club)
                .Where(cs => cs.UserId == userId)
                .ToListAsync();

            return clubStaff ?? new();
        }

        public async Task CreateRangeAsync(long clubId, List<long> userIds)
        {
            var staffToCreate = new List<ClubStaffEntity>();
            foreach (var userId in userIds)
            {
                staffToCreate.Add(new(clubId, userId));
            }

            await _context.ClubStaff.AddRangeAsync(staffToCreate);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRangeAsync(long clubId, List<long> userIds)
        {
            var staffToDelete = new List<ClubStaffEntity>();
            foreach (var userId in userIds)
            {
                var staff = await GetClubStaff(clubId, userId);
                staffToDelete.Add(staff);
            }

            _context.ClubStaff.RemoveRange(staffToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
