using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Dto.Users.Creation;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.User
{
    public class UserMembershipDbService : IUserMembershipDbService
    {
        private readonly AppDbContext _context;

        public UserMembershipDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserMembershipEntity>> GetActiveUserMembershipsAsync(long userId)
        {
            return await _context.UserMemberships
                .Where(um => um.UserId == userId
                && um.ClosedAt == null)
                .Include(um => um.User)
                .Include(um => um.Group)
                    .ThenInclude(g => g.UserMemberships)
                        .ThenInclude(um => um.User)
                .Include(um => um.Group)
                    .ThenInclude(g => g.Club)
                .Include(um => um.Attendances)
                .OrderByDescending(ug => ug.CreateAt)
                .ToListAsync();
        }

        public async Task<List<UserMembershipEntity>> GetActiveUserMembershipsAsUserAsync(long userId)
        {
            return await _context.UserMemberships
                .Where(um => um.UserId == userId
                && um.ClosedAt == null
                && um.RoleInGroup == Role.User)
                .Include(um => um.User)
                .Include(um => um.Group)
                    .ThenInclude(g => g.UserMemberships)
                        .ThenInclude(um => um.User)
                .Include(um => um.Group)
                    .ThenInclude(g => g.Club)
                .Include(um => um.Attendances)
                .OrderByDescending(ug => ug.CreateAt)
                .ToListAsync();
        }

        public UserMembershipEntity GetActiveUserMembership(long userId, long groupId)
        {
            var entity = _context.UserMemberships
                .Include(um => um.User)
                .Include(um => um.Attendances)
                .Include(um => um.Group)
                    .ThenInclude(g => g.UserMemberships)
                .Include(um => um.Group)
                    .ThenInclude(g => g.Club)
                .FirstOrDefault(um =>
                    um.UserId == userId &&
                    um.GroupId == groupId &&
                    um.ClosedAt == null);

            return entity ?? throw new EntityNotFoundException(nameof(UserMembershipEntity));
        }

        public UserMembershipEntity GetMainUserMembership(long userId)
        {
            var mainUserMembership = _context.UserMemberships.AsQueryable()
                .Where(um => um.IsMain == true
                && um.UserId == userId
                && um.ClosedAt == null)
                .Include(um => um.User)
                .Include(um => um.Club)
                    .ThenInclude(um => um.Manager)
                .Include(um => um.Group)
                    .ThenInclude(um => um.UserMemberships)
                        .ThenInclude(um => um.User)
                .FirstOrDefault();

            return mainUserMembership;
        }

        public async Task<bool> UserMembershipExists(long userId, long groupId)
        {
            var userMembership = await _context.UserMemberships.Where(um => um.UserId == userId
                && um.GroupId == groupId
                && um.ClosedAt == null)
                .FirstOrDefaultAsync();

            return userMembership != null;
        }

        public async Task<long> CreateUserMembershipAsync(long userId, UserMembershipCreationDto userMembership)
        {
            var userMembershipEntity = new UserMembershipEntity(userId, userMembership);
            await _context.UserMemberships.AddAsync(userMembershipEntity);
            await _context.SaveChangesAsync();

            return userMembershipEntity.Id;
        }

        public async Task UpdateUserMembershipAsync(long userId, UserMembershipCreationDto userMembership)
        {
            var clubId = userMembership.ClubId;
            var groupId = userMembership.GroupId;

            var existingMembership = await _context.UserMemberships
                .FirstAsync(um => um.UserId == userId
                && um.ClubId == clubId
                && um.GroupId == groupId
                && um.ClosedAt == null);

            existingMembership.IsMain = userMembership.IsMain;

            _context.Update(existingMembership);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserMembershipAsync(UserMembershipEntity userMembership)
        {
            _context.UserMemberships.Update(userMembership);
            await _context.SaveChangesAsync();
        }

        public async Task CloseUserMembershipAsync(long userId, long groupId)
        {
            var userMembership = await _context.UserMemberships
                .Where(um => um.UserId == userId
                && um.GroupId == groupId
                && um.ClosedAt == null)
                .FirstOrDefaultAsync();

            if (userMembership == null)
            {
                throw new EntityNotFoundException(nameof(UserMembershipEntity));
            }

            userMembership.ClosedAt = DateTime.UtcNow;

            _context.UserMemberships.Update(userMembership);
            await _context.SaveChangesAsync();
        }

        public async Task RecoverUserMembershipAsync(long userId, long groupId)
        {
            var userMembership = await _context.UserMemberships
                .Where(um => um.UserId == userId
                && um.GroupId == groupId)
                .FirstOrDefaultAsync();

            if (userMembership == null)
            {
                throw new EntityNotFoundException(nameof(UserMembershipEntity));
            }

            userMembership.ClosedAt = null;

            _context.UserMemberships.Update(userMembership);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserMembershipAsync(long userId, long groupId)
        {
            var userMembership = await _context.UserMemberships
                .FirstOrDefaultAsync(um => um.UserId == userId && um.GroupId == groupId);

            _context.Remove(userMembership);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserMemberships(long userId)
        {
            var userMemberships = await _context.UserMemberships
                .AsQueryable()
                .Where(um => um.UserId == userId)
                .ToListAsync();

            foreach (var userMembership in userMemberships)
            {
                _context.Remove(userMembership);
            }

            var user = await _context.Users.FindAsync(userId);
            user.MainUserMembershipAsUserId = null;

            await _context.SaveChangesAsync();
        }
    }
}
