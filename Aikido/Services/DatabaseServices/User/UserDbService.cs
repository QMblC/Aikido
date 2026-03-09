using Aikido.AdditionalData.Enums;
using Aikido.Data;
using Aikido.Dto.Users;
using Aikido.Dto.Users.Creation;
using Aikido.Entities;
using Aikido.Entities.Filters;
using Aikido.Entities.Users;
using Aikido.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services.DatabaseServices.User
{
    public class UserDbService : IUserDbService
    {
        private readonly AppDbContext _context;

        public UserDbService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserEntity> GetByIdOrThrowException(long id)
        {
            var user = await _context.Users
                .Include(u => u.UserMemberships)
                    .ThenInclude(uc => uc.Club)
                .Include(u => u.UserMemberships)
                    .ThenInclude(ug => ug.Group)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException($"Пользователь с Id = {id} не найден");
            }
            return user;
        }

        public async Task<bool> Exists(long id)
        {
            return await _context.Users.AnyAsync(u => u.Id == id);
        }

        public async Task<List<UserShortDto>> GetUserIdAndNamesAsync()
        {
            var users = await _context.Users
            .Include(u => u.UserMemberships)
                .ThenInclude(um => um.Club)
            .Include(u => u.UserMemberships)
                .ThenInclude(um => um.Group)
            .Select(u => new UserShortDto(u))
            .ToListAsync();

            return users;
        }

        public async Task<List<UserEntity>> GetManagers()
        {
            var managers = await _context.Users.AsQueryable()
                .Where(u => u.Role == Role.Manager)
                .Include(u => u.UserMemberships)
                    .ThenInclude(um => um.Club)
                .Include(u => u.UserMemberships)
                    .ThenInclude(um => um.Group)
                .ToListAsync();

            return managers;
        }

        public async Task<List<UserEntity>> GetCoachStudentByName(long coachId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<UserEntity>();

            name = name.ToLower();

            var members = await _context.UserMemberships
                .Where(um => um.Group.UserMemberships.Any(um2 => um2.UserId == coachId))
                .Select(um => um.User)
                .Where(u => u.Id != coachId)
                .Where(u =>
                    (u.LastName != null && u.LastName.ToLower().Contains(name)) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(name)) ||
                    (u.MiddleName != null && u.MiddleName.ToLower().Contains(name)))
                .Distinct()
                .ToListAsync();

            return members;
        }

        public async Task<(List<UserDto> Users, int TotalCount)> GetUserListAlphabetAscending(int startIndex, int finishIndex, UserFilter filter)
        {
            var query = _context.Users
                .Include(u => u.UserMemberships)
                    .ThenInclude(uc => uc.Club)
                .Include(u => u.UserMemberships)
                    .ThenInclude(ug => ug.Group)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var nameLower = filter.Name.ToLower();
                query = query.Where(u =>
                    ((u.LastName ?? "") + " " + (u.FirstName ?? "") + " " + (u.MiddleName ?? ""))
                    .ToLower()
                    .Contains(nameLower)
                );
            }

            if (filter.Roles != null && filter.Roles.Any())
            {
                var roleEnums = filter.Roles.Select(EnumParser.ConvertStringToEnum<Role>).ToList();
                query = query.Where(u => roleEnums.Contains(u.Role));
            }

            if (filter.Grades != null && filter.Grades.Any())
            {
                var gradeEnums = filter.Grades.Select(EnumParser.ConvertStringToEnum<Grade>).ToList();
                query = query.Where(u => gradeEnums.Contains(u.Grade));
            }

            if (filter.ClubIds != null && filter.ClubIds.Any())
            {
                query = query.Where(u => u.UserMemberships.Any(uc => filter.ClubIds.Contains(uc.ClubId)));
            }

            if (filter.GroupIds != null && filter.GroupIds.Any())
            {
                query = query.Where(u => u.UserMemberships.Any(ug => filter.GroupIds.Contains(ug.GroupId)));
            }

            if (filter.Cities != null && filter.Cities.Any())
            {
                var lowerCities = filter.Cities.Select(c => c.ToLower()).ToList();
                query = query.Where(u => u.City != null && lowerCities.Contains(u.City.ToLower()));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ThenBy(u => u.MiddleName)
                .Skip(startIndex)
                .Take(finishIndex - startIndex)
                .ToListAsync();

            var userDtos = users.Select(u => new UserDto(u, u.UserMemberships.ToList())).ToList();

            return (userDtos, totalCount);
        }

        public async Task<long> CreateUser(UserCreationDto userData)
        {
            var user = new UserEntity(userData);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        public async Task<List<long>> CreateUsers(List<UserCreationDto> users)
        {
            var entities = users.Select(u => new UserEntity(u)).ToList();
            _context.Users.AddRange(entities);
            await _context.SaveChangesAsync();
            return entities.Select(e => e.Id).ToList();
        }

        public async Task UpdateUser(long id, UserCreationDto userData)
        {
            var user = await GetByIdOrThrowException(id);
            user.Update(userData);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(UserEntity user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUsers(List<UserDto> users)
        {
            foreach (var userData in users)
            {
                if (userData.Id.HasValue)
                {
                    var user = await GetByIdOrThrowException(userData.Id.Value);
                    user.Update(userData);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task Delete(long id)
        {
            var user = await GetByIdOrThrowException(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
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

        public async Task UpdateUserGrade(long userId, Grade grade)
        {
            var user = await _context.Users.FindAsync(userId);

            user.Grade = grade;
            _context.Update(user);

            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserBudoPassport(long userId, bool value)
        {
            var user = await _context.Users.FindAsync(userId);

            user.HasBudoPassport = value;
            _context.Update(user);

            await _context.SaveChangesAsync();
        }

        public async Task<List<UserEntity>> FindClubMemberByName(long clubId, string name)
        {
            var query = _context.Users
                .Where(u => u.UserMemberships.Any(um => um.IsMain
                && um.ClubId == clubId
                && um.RoleInGroup == Role.User))
                .Include(u => u.UserMemberships)
                    .ThenInclude(uc => uc.Club)
                .Include(u => u.UserMemberships)
                    .ThenInclude(ug => ug.Group)
                .AsQueryable();
                
            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameLower = name.ToLower();
                query = query.Where(u =>
                    ((u.LastName ?? "") + " " + (u.FirstName ?? "") + " " + (u.MiddleName ?? ""))
                    .ToLower()
                    .Contains(nameLower)
                );
            }

            return await query.ToListAsync();
        }

        public async Task<List<UserEntity>> FindCoachMemberInClubByName(long clubId, long coachId, string name)
        {
            var query = _context.Users
                .Include(u => u.UserMemberships)
                    .ThenInclude(um => um.Club)
                .Include(u => u.UserMemberships)
                    .ThenInclude(um => um.Group)
                .Where(u => u.UserMemberships.Any(um => um.IsMain
                    && um.ClubId == clubId
                    && um.RoleInGroup == Role.User
                    && um.Group.UserMemberships.Any(um2 => um2.UserId == coachId && um2.RoleInGroup == Role.Coach)))
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(name))
            {
                var nameLower = name.ToLower();
                query = query.Where(u =>
                    ((u.LastName ?? "") + " " + (u.FirstName ?? "") + " " + (u.MiddleName ?? ""))
                    .ToLower()
                    .Contains(nameLower)
                );
            }

            return await query.ToListAsync();
        }
    }
}