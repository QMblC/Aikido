using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Entities.Filters;
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
                .Include(u => u.UserClubs)
                    .ThenInclude(uc => uc.Club)
                .Include(u => u.UserGroups)
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
            return await _context.Users
                .Include(u => u.UserClubs)
                    .ThenInclude(uc => uc.Club)
                .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                .Select(u => new UserShortDto
                {
                    Id = u.Id,
                    Name = u.FullName,
                    Role = u.Role.ToString(),
                    Grade = u.Grade.ToString(),
                    PhoneNumber = u.PhoneNumber,
                    City = u.City,
                    ClubNames = u.UserClubs.Where(uc => uc.IsActive && uc.Club != null)
                                          .Select(uc => uc.Club!.Name).ToList(),
                    GroupNames = u.UserGroups.Where(ug => ug.IsActive && ug.Group != null)
                                            .Select(ug => ug.Group!.Name).ToList()
                })
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<(List<UserDto> Users, int TotalCount)> GetUserListAlphabetAscending(int startIndex, int finishIndex, UserFilter filter)
        {
            var query = _context.Users
                .Include(u => u.UserClubs)
                    .ThenInclude(uc => uc.Club)
                .Include(u => u.UserGroups)
                    .ThenInclude(ug => ug.Group)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var nameLower = filter.Name.ToLower();
                query = query.Where(u => u.FullName != null && u.FullName.ToLower().Contains(nameLower));
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
                query = query.Where(u => u.UserClubs.Any(uc => filter.ClubIds.Contains(uc.ClubId) && uc.IsActive));
            }

            if (filter.GroupIds != null && filter.GroupIds.Any())
            {
                query = query.Where(u => u.UserGroups.Any(ug => filter.GroupIds.Contains(ug.GroupId) && ug.IsActive));
            }

            if (filter.Cities != null && filter.Cities.Any())
            {
                var lowerCities = filter.Cities.Select(c => c.ToLower()).ToList();
                query = query.Where(u => u.City != null && lowerCities.Contains(u.City.ToLower()));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip(startIndex)
                .Take(finishIndex - startIndex)
                .ToListAsync();

            var userDtos = users.Select(u => new UserDto(u,
                u.UserClubs.ToList(),
                u.UserGroups.ToList())).ToList();

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

        // Методы для работы с клубами
        public async Task<List<UserClub>> GetUserClubsAsync(long userId)
        {
            return await _context.UserClubs
                .Include(uc => uc.Club)
                .Where(uc => uc.UserId == userId)
                .OrderByDescending(uc => uc.JoinDate)
                .ToListAsync();
        }

        public async Task AddUserToClubAsync(long userId, long clubId, string membershipType = "Regular")
        {
            var existingMembership = await _context.UserClubs
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClubId == clubId);

            if (existingMembership == null)
            {
                var userClub = new UserClub(userId, clubId)
                {
                    MembershipType = membershipType
                };
                _context.UserClubs.Add(userClub);
                await _context.SaveChangesAsync();
            }
            else if (!existingMembership.IsActive)
            {
                existingMembership.IsActive = true;
                existingMembership.JoinDate = DateTime.UtcNow;
                existingMembership.LeaveDate = null;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserFromClubAsync(long userId, long clubId)
        {
            var userClub = await _context.UserClubs
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClubId == clubId);

            if (userClub != null)
            {
                userClub.IsActive = false;
                userClub.LeaveDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserFromAllClubsAsync(long userId)
        {
            var userClubs = await _context.UserClubs
                .Where(uc => uc.UserId == userId && uc.IsActive)
                .ToListAsync();

            foreach (var userClub in userClubs)
            {
                userClub.IsActive = false;
                userClub.LeaveDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // Методы для работы с группами
        public async Task<List<UserGroup>> GetUserGroupsAsync(long userId)
        {
            return await _context.UserGroups
                .Include(ug => ug.Group)
                .Where(ug => ug.UserId == userId)
                .OrderByDescending(ug => ug.JoinDate)
                .ToListAsync();
        }

        public async Task AddUserToGroupAsync(long userId, long groupId, Role roleInGroup = Role.User)
        {
            var existingMembership = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (existingMembership == null)
            {
                var userGroup = new UserGroup(userId, groupId, roleInGroup);
                _context.UserGroups.Add(userGroup);
                await _context.SaveChangesAsync();
            }
            else if (!existingMembership.IsActive)
            {
                existingMembership.IsActive = true;
                existingMembership.JoinDate = DateTime.UtcNow;
                existingMembership.LeaveDate = null;
                existingMembership.RoleInGroup = roleInGroup;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserFromGroupAsync(long userId, long groupId)
        {
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (userGroup != null)
            {
                userGroup.IsActive = false;
                userGroup.LeaveDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserFromAllGroupsAsync(long userId)
        {
            var userGroups = await _context.UserGroups
                .Where(ug => ug.UserId == userId && ug.IsActive)
                .ToListAsync();

            foreach (var userGroup in userGroups)
            {
                userGroup.IsActive = false;
                userGroup.LeaveDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserClubMembershipAsync(long userId, long clubId, UserClubDto membershipInfo)
        {
            var userClub = await _context.UserClubs
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.ClubId == clubId);

            if (userClub != null)
            {
                userClub.MembershipType = membershipInfo.MembershipType;
                userClub.MembershipFee = membershipInfo.MembershipFee;
                userClub.LastPaymentDate = membershipInfo.LastPaymentDate;
                userClub.Notes = membershipInfo.Notes;
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateUserGroupRoleAsync(long userId, long groupId, UserGroupDto groupInfo)
        {
            var userGroup = await _context.UserGroups
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GroupId == groupId);

            if (userGroup != null)
            {
                userGroup.RoleInGroup = EnumParser.ConvertStringToEnum<Role>(groupInfo.RoleInGroup);
                userGroup.Notes = groupInfo.Notes;
                userGroup.IsRegular = groupInfo.IsRegular;
                await _context.SaveChangesAsync();
            }
        }
    }
}