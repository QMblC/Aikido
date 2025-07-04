using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Aikido.Entities.Filters;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Aikido.Services
{
    public class PagedUserResult
    {
        public int TotalCount { get; set; }
        public List<UserEntity> Users { get; set; } = [];
    }


    public class UserService
    {
        private readonly AppDbContext context;

        public UserService(AppDbContext context)
        {
            this.context = context;
        }

        private async Task SaveDb()
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при обработке пользователя: {ex.InnerException?.Message}", ex);
            }
        }

        public async Task<List<UserShortDto>> GetUserIdAndNamesAsync()
        {
            try
            {
                return await context.Users
                    .Select(u => new UserShortDto { Id = u.Id, Name = u.FullName })
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

        private async Task EnsureLoginIsUnique(string? login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return;

            var loginExists = await context.Users.AnyAsync(u => u.Login == login);
            if (loginExists)
                throw new Exception($"Пользователь с логином '{login}' уже существует.");
        }


        public async Task DeleteUser(long id)
        {
            var userEntity = await context.Users.FindAsync(id);
            if (userEntity == null)
                throw new KeyNotFoundException($"Пользователь с Id = {id} не найден.");

            context.Remove(userEntity);

            await SaveDb();

        }
        public async Task UpdateUser(long id, UserDto userNewData)
        {
            await EnsureLoginIsUnique(userNewData.Login);

            var userEntity = await context.Users.FindAsync(id);
            if (userEntity == null)
                throw new KeyNotFoundException($"Пользователь с Id = {id} не найден.");

            userEntity.UpdateFromJson(userNewData);

            await SaveDb();
        }

        public async Task<PagedUserResult> GetUserListAlphabetAscending(
            int startIndex,
            int finishIndex,
            UserFilter? filter = null)
        {
            var query = ApplyUserFilters(context.Users.AsQueryable(), filter);

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip(startIndex)
                .Take(finishIndex - startIndex)
                .Select(ProjectToUserEntity())
                .ToListAsync();

            return new PagedUserResult
            {
                TotalCount = totalCount,
                Users = users
            };
        }

        private IQueryable<UserEntity> ApplyUserFilters(IQueryable<UserEntity> query, UserFilter? filter)
        {
            if (filter == null) return query;

            if (!string.IsNullOrEmpty(filter.Role))
                query = query.Where(u => u.Role == filter.Role);

            if (!string.IsNullOrEmpty(filter.City))
                query = query.Where(u => u.City == filter.City);

            if (!string.IsNullOrEmpty(filter.Grade))
                query = query.Where(u => u.Grade == filter.Grade);

            if (filter.ClubId.HasValue)
                query = query.Where(u => u.ClubId == filter.ClubId);

            if (filter.GroupId.HasValue)
                query = query.Where(u => u.GroupId == filter.GroupId);

            if (!string.IsNullOrEmpty(filter.Sex))
                query = query.Where(u => u.Sex == filter.Sex);

            return query;
        }

        private static Expression<Func<UserEntity, UserEntity>> ProjectToUserEntity()
        {
            return u => new UserEntity
            {
                Id = u.Id,
                FullName = u.FullName,
                Photo = u.Photo,
                Login = u.Login,
                Role = u.Role,
                City = u.City,
                Birthday = u.Birthday,
                Grade = u.Grade,
                ClubId = u.ClubId,
                GroupId = u.GroupId
            };
        }


    }
}
