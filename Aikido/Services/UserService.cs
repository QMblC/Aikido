using Aikido.AdditionalData;
using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Entities.Filters;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    public class PagedUserResult
    {
        public int TotalCount { get; set; }
        public List<UserDto> Users { get; set; } = [];
    }

    public class UserService : DbService
    {
        public UserService(AppDbContext context) : base(context) { }

        public async Task<List<UserShortDto>> GetUserIdAndNamesAsync()
        {
            try
            {
                return await context.Users
                    .Select(u => new UserShortDto(u))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Не удалось получить список пользователей.", ex);
            }
        }

        public async Task<UserEntity> GetUserById(long id)
        {
            var userEntity = await context.Users.FindAsync(id);
            if (userEntity == null)
                throw new KeyNotFoundException($"Пользователь с Id = {id} не найден.");

            return userEntity;
        }

        public async Task<long> CreateUser(UserDto userData)
        {
            await EnsureLoginIsUnique(userData.Login);

            var userEntity = new UserEntity();
            userEntity.UpdateFromJson(userData);

            context.Users.Add(userEntity);
            await SaveDb();

            return userEntity.Id;
        }

        public async Task<List<long>> CreateUsers(List<UserDto> usersData)
        {
            var createdUserIds = new List<long>();
            var loginsToCheck = usersData
                .Select(u => u.Login?.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            var duplicatesInRequest = loginsToCheck
                .GroupBy(l => l)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatesInRequest.Any())
                throw new Exception($"Дубликаты логинов в запросе: {string.Join(", ", duplicatesInRequest)}");

            var existingLogins = await context.Users
                .Where(u => loginsToCheck.Contains(u.Login))
                .Select(u => u.Login)
                .ToListAsync();

            if (existingLogins.Any())
                throw new Exception($"Следующие логины уже существуют: {string.Join(", ", existingLogins)}");

            foreach (var userData in usersData)
            {
                var userEntity = new UserEntity();



                userEntity.UpdateFromJson(userData);
                context.Users.Add(userEntity);
            }

            await SaveDb();

            return context.Users
                .Where(u => loginsToCheck.Contains(u.Login))
                .Select(u => u.Id)
                .ToList();
        }

        private async Task EnsureLoginIsUnique(string? login, long? excludeUserId = null)
        {
            if (string.IsNullOrWhiteSpace(login))
                return;

            var loginExists = await context.Users
                .AnyAsync(u => u.Login == login && (!excludeUserId.HasValue || u.Id != excludeUserId.Value));

            if (loginExists)
                throw new Exception($"Пользователь с логином '{login}' уже существует.");
        }

        public async Task DeleteUser(long id, bool saveDb = true)
        {
            var userEntity = await context.Users.FindAsync(id);
            if (userEntity == null)
                throw new KeyNotFoundException($"Пользователь с Id = {id} не найден.");

            context.Users.Remove(userEntity);

            if (saveDb)
                await SaveDb();

        }
        public async Task UpdateUser(long id, UserDto userNewData)
        {
            await EnsureLoginIsUnique(userNewData.Login, id);

            var userEntity = await context.Users.FindAsync(id);
            if (userEntity == null)
                throw new KeyNotFoundException($"Пользователь с Id = {id} не найден.");

            userEntity.UpdateFromJson(userNewData);

            await SaveDb();
        }

        public async Task UpdateUsers(List<UserDto> usersData)
        {
            var idsToUpdate = usersData
                .Select(u => u.Id)
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToList();

            if (idsToUpdate.Count != usersData.Count)
                throw new Exception("Некоторые записи не содержат Id.");

            var existingUsers = await context.Users
                .Where(u => idsToUpdate.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var loginsToCheck = usersData
                .Select(u => u.Login?.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            var duplicatesInRequest = loginsToCheck
                .GroupBy(l => l)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatesInRequest.Any())
                throw new Exception($"Дубликаты логинов в списке: {string.Join(", ", duplicatesInRequest)}");

            foreach (var userDto in usersData)
            {
                if (!userDto.Id.HasValue || !existingUsers.TryGetValue(userDto.Id.Value, out var userEntity))
                    throw new Exception($"Пользователь с Id = {userDto.Id} не найден.");

                await EnsureLoginIsUnique(userDto.Login, userDto.Id);

                if (userDto.Photo == null)
                    userDto.Photo = Convert.ToBase64String(userEntity.Photo);

                userEntity.UpdateFromJson(userDto);
            }

            await SaveDb();
        }

        public async Task<List<UserEntity>> GetClubMembers(long clubId)
        {
            return await context.Users
                .Where(user => user.ClubId == clubId)
                .ToListAsync();
        }

        public async Task<PagedUserResult> GetUserListAlphabetAscending(
            int startIndex,
            int finishIndex,
            UserFilter? filter = null)
        {
            var query = ApplyUserFilters(context.Users.AsQueryable(), filter);

            var totalCount = await query.CountAsync();

            var usersEntities = await query
                .OrderBy(u => u.FullName)
                .Skip(startIndex)
                .Take(finishIndex - startIndex)
                .ToListAsync();

            var users = usersEntities
                .Select(user => new UserDto(user))
                .ToList();

            return new PagedUserResult
            {
                TotalCount = totalCount,
                Users = users
            };
        }


        private IQueryable<UserEntity> ApplyUserFilters(IQueryable<UserEntity> query, UserFilter? filter)
        {
            if (filter == null) return query;

            if (filter.Roles?.Any() == true)
            {
                var enumRoles = filter.Roles
                    .Select(EnumParser.ConvertStringToEnum<Role>)
                    .ToList();

                query = query.Where(u => enumRoles.Contains(u.Role));
            }

            if (filter.Cities?.Any() == true)
            {
                query = query.Where(u => filter.Cities.Contains(u.City));
            }

            if (filter.Grades?.Any() == true)
            {
                var enumGrades = filter.Grades
                    .Select(EnumParser.ConvertStringToEnum<Grade>)
                    .ToList();

                query = query.Where(u => enumGrades.Contains(u.Grade));
            }

            if (filter.ClubIds?.Any() == true)
            {
                query = query.Where(u => u.ClubId.HasValue && filter.ClubIds.Contains(u.ClubId.Value));
            }

            if (filter.GroupIds?.Any() == true)
            {
                query = query.Where(u => u.GroupId.HasValue && filter.GroupIds.Contains(u.GroupId.Value));
            }

            if (filter.Sex?.Any() == true)
            {
                var enumSexes = filter.Sex
                    .Select(EnumParser.ConvertStringToEnum<Sex>)
                    .ToList();

                query = query.Where(u => enumSexes.Contains(u.Sex));
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var lowered = filter.Name.ToLower();
                query = query.Where(u => u.FullName.ToLower().StartsWith(lowered));
            }

            return query;
        }
    }
}
