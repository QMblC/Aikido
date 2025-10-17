using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
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
                .Select(u => new UserShortDto
                {
                    Id = u.Id,
                    LastName = u.LastName,
                    FirstName = u.FirstName,
                    MiddleName = u.SecondName,
                    Role = u.Role.ToString(),
                    Grade = u.Grade.ToString(),
                    PhoneNumber = u.PhoneNumber,
                    City = u.City,
                    ClubNames = u.UserMemberships.Where(um => um.Club != null)
                                          .Select(um => um.Club!.Name).ToList(),
                    GroupNames = u.UserMemberships.Where(um => um.Group != null)
                                            .Select(um => um.Group!.Name).ToList()
                })
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ThenBy(u => u.MiddleName)
                .ToListAsync();
            return users;
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
                    (u.LastName != null && u.LastName.ToLower().Contains(nameLower)) ||
                    (u.FirstName != null && u.FirstName.ToLower().Contains(nameLower)) ||
                    (u.SecondName != null && u.SecondName.ToLower().Contains(nameLower))
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
                .ThenBy(u => u.SecondName)
                .Skip(startIndex)
                .Take(finishIndex - startIndex)
                .ToListAsync();

            var userDtos = users.Select(u => new UserDto(u, u.UserMemberships.ToList())).ToList();

            return (userDtos, totalCount);
        }

        public async Task<long> CreateUser(UserDto userData)
        {
            var user = new UserEntity(userData);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        public async Task<List<long>> CreateUsers(List<UserDto> users)
        {
            var entities = users.Select(u => new UserEntity(u)).ToList();
            _context.Users.AddRange(entities);
            await _context.SaveChangesAsync();
            return entities.Select(e => e.Id).ToList();
        }

        public async Task UpdateUser(long id, UserDto userData)
        {
            var user = await GetByIdOrThrowException(id);
            user.UpdateFromJson(userData);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUsers(List<UserDto> users)
        {
            foreach (var userData in users)
            {
                if (userData.Id.HasValue)
                {
                    var user = await GetByIdOrThrowException(userData.Id.Value);
                    user.UpdateFromJson(userData);
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

        // Методы для работы с группами
        public async Task<List<UserMembershipEntity>> GetUserMembershipsAsync(long userId)
        {
            return await _context.UserMemberships
                .Include(ug => ug.Group)
                    .ThenInclude(g => g.Coach)
                .Include(ug => ug.Group)
                    .ThenInclude(g => g.Club)
                .Where(ug => ug.UserId == userId)
                .OrderByDescending(ug => ug.JoinDate)
                .ToListAsync();
        }

        public async Task AddUserMembershipAsync(long userId, long groupId, long clubId, Role roleInGroup = Role.User)
        {
            var existingMembership = await _context.UserMemberships
                .FirstOrDefaultAsync(um => um.UserId == userId && um.ClubId == clubId && um.GroupId == groupId);

            if (existingMembership == null)
            {
                var userMembership = new UserMembershipEntity(userId, groupId, clubId, roleInGroup);
                _context.UserMemberships.Add(userMembership);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserMembershipAsync(long userId, long groupId)
        {
            var userMembership = await _context.UserMemberships
                .FirstOrDefaultAsync(um => um.UserId == userId && um.GroupId == groupId);

            if (userMembership != null)
            {
                _context.Remove(userMembership);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserMemberships(long userId)
        {
            var userMemberships = await _context.UserMemberships
                .Where(um => um.UserId == userId)
                .ToListAsync();

            foreach (var userMembership in userMemberships)
            {
                _context.Remove(userMembership);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserMembershipAsync(long userId, long groupId, UserMembershipDto membershipInfo)
        {
            var userMembership = await _context.UserMemberships
                .FirstOrDefaultAsync(um => um.UserId == userId && um.GroupId == groupId);

            if (userMembership != null)
            {
                userMembership.RoleInGroup = EnumParser.ConvertStringToEnum<Role>(membershipInfo.RoleInGroup);
                await _context.SaveChangesAsync();
            }
        }
    }
}