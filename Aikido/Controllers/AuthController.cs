using Aikido.Dto;
using Aikido.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService authService;

        public AuthController(AuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await authService.LoginAsync(dto);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Ошибка при входе");
            }
        }
    }

}
