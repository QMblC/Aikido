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

        public async Task<UserEntity?> GetUserDataById(long id)
        {
            return await context.Users.FindAsync(id);
        }
    }
}
