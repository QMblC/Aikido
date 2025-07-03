using Aikido.Data;
using Aikido.Dto;
using Aikido.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aikido.Services
{
    

    public class AuthService
    {
        private readonly AppDbContext context;
        private readonly JwtService jwtService;

        public AuthService(AppDbContext context, JwtService jwtService)
        {
            this.context = context;
            this.jwtService = jwtService;
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Login == dto.Login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                throw new UnauthorizedAccessException("Неверный логин или пароль");

            return jwtService.GenerateToken(user);
        }
    }

}
