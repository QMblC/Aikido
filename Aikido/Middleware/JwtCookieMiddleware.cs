using Aikido.Services;
using Microsoft.AspNetCore.Authentication;

namespace Aikido.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;
        private const string AccessTokenCookie = "AccessToken";

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, JwtService jwtService)
        {
            var token = context.Request.Cookies[AccessTokenCookie];

            if (!string.IsNullOrEmpty(token))
            {
                var principal = jwtService.ValidateAccessToken(token);

                if (principal != null)
                {
                    context.User = principal;
                }
            }

            await _next(context);
        }
    }

    public static class JwtCookieMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookie(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieMiddleware>();
        }
    }
}
