using Aikido.Data;
using Aikido.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Services
{
    public class UserService
    {
        private readonly AppDbContext context;

        public UserService(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<UserEntity> GetUserDataById(long id)
        {
            var userEntity = await context.Users.FindAsync(id);
            if (userEntity == null)
                throw new KeyNotFoundException($"Пользователь с Id = {id} не найден.");

            return userEntity;
        }

        public async Task<long> CreateUser(UserJson userData)
        {
            var userEntity = new UserEntity();
            userEntity.UpdateFromJson(userData);

            context.Users.Add(userEntity);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при сохранении пользователя: " + ex.InnerException?.Message, ex);
            }

            return userEntity.Id;
        }

        public async Task DeleteUser(long id)
        {
            var userEntity = await context.Users.FindAsync(id);
            if (userEntity == null)
                throw new KeyNotFoundException($"Пользователь с Id = {id} не найден.");

            context.Remove(userEntity);
            await context.SaveChangesAsync();

            return;
        }
        public async Task UpdateUser(long id, UserJson userNewData)
        {
            var userEntity = await context.Users.FindAsync(id);
            if (userEntity == null)
                throw new KeyNotFoundException($"Пользователь с Id = {id} не найден.");

            userEntity.UpdateFromJson(userNewData);

            await context.SaveChangesAsync();
        }

    }
}
