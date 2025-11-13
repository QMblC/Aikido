using Aikido.Application.Services;
using Aikido.Dto.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aikido.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthApplicationService _authService;
        private const string AccessTokenCookie = "AccessToken";
        private const string RefreshTokenCookie = "RefreshToken";

        public AuthController(AuthApplicationService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Вход в систему
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.AuthenticateAsync(
                    request.Login,
                    request.Password,
                    request.DeviceInfo);

                if (result == null)
                    return Unauthorized(new { Message = "Неверный логин или пароль" });

                var (user, accessToken, refreshToken) = result.Value;

                SetAccessTokenCookie(accessToken);

                SetRefreshTokenCookie(refreshToken.Token);

                return Ok(new AuthResponseDto
                {
                    UserId = user.Id,
                    Login = user.Login ?? string.Empty,
                    Role = user.Role.ToString(),
                    FullName = user.FullName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при входе в систему", Details = ex.Message });
            }
        }

        /// <summary>
        /// Обновление Access Token с использованием Refresh Token
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                var refreshToken = Request.Cookies[RefreshTokenCookie];

                if (string.IsNullOrEmpty(refreshToken))
                    return Unauthorized(new { Message = "Refresh token отсутствует" });

                var result = await _authService.RefreshTokenAsync(refreshToken);

                if (result == null)
                {
                    ClearAuthCookies();
                    return Unauthorized(new { Message = "Недействительный или истекший refresh token" });
                }

                var (newAccessToken, newRefreshToken) = result.Value;

                SetAccessTokenCookie(newAccessToken);
                SetRefreshTokenCookie(newRefreshToken.Token);

                return Ok(new { Message = "Токены успешно обновлены" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при обновлении токена", Details = ex.Message });
            }
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var refreshToken = Request.Cookies[RefreshTokenCookie];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    await _authService.RevokeTokenAsync(refreshToken);
                }

                ClearAuthCookies();

                return Ok(new { Message = "Вы успешно вышли из системы" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при выходе", Details = ex.Message });
            }
        }

        /// <summary>
        /// Выход со всех устройств (отзыв всех refresh tokens пользователя)
        /// </summary>
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized();

                await _authService.RevokeAllUserTokensAsync(userId);
                ClearAuthCookies();

                return Ok(new { Message = "Вы вышли со всех устройств" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ошибка при выходе", Details = ex.Message });
            }
        }

        /// <summary>
        /// Проверка статуса аутентификации
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetAuthStatus()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);

                return Ok(new
                {
                    IsAuthenticated = true,
                    UserId = userIdClaim?.Value,
                    Role = roleClaim?.Value
                });
            }

            return Ok(new { IsAuthenticated = false });
        }

        private void SetAccessTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15),
                Path = "/"
            };

            Response.Cookies.Append(AccessTokenCookie, token, cookieOptions);
        }

        private void SetRefreshTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(14),
                Path = "/api/auth/refresh"
            };

            Response.Cookies.Append(RefreshTokenCookie, token, cookieOptions);
        }

        private void ClearAuthCookies()
        {
            Response.Cookies.Delete(AccessTokenCookie, new CookieOptions { Path = "/" });
            Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions { Path = "/api/auth/refresh" });
        }
    }
}
