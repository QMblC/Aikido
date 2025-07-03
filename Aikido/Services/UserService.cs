using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
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
            var userEntity = new UserEntity();
            userEntity.UpdateFromJson(userData);

            context.Users.Add(userEntity);

            await SaveDb();

            return userEntity.Id;
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
            var userEntity = await context.Users.FindAsync(id);
            if (userEntity == null)
                throw new KeyNotFoundException($"Пользователь с Id = {id} не найден.");

            userEntity.UpdateFromJson(userNewData);

            await SaveDb();
        }

        public async Task<List<UserEntity>> GetUserListAlphabetAscending(int startIndex, int finishIndex)
        {
            return await context.Users
                .OrderBy(u => u.FullName)
                .Skip(startIndex)
                .Take(finishIndex - startIndex)
                .Select(u => new UserEntity
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Photo = u.Photo,
                    Login = u.Login,
                    Role = u.Role,
                    City = u.City,
                    Birthday = u.Birthday,
                    Grade = u.Grade,
                    GroupId = u.GroupId
                })
                .ToListAsync();
        }


    }
}
