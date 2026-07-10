using DigitalProject.Security;

namespace DigitalProject.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITokenBlacklistService _blacklistService;

        public TokenBlacklistMiddleware(
            RequestDelegate next,
            ITokenBlacklistService blacklistService)
        {
            _next = next;
            _blacklistService = blacklistService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();

                if (_blacklistService.IsBlacklisted(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { error = "token_revoked" });
                    return;
                }
            }

            await _next(context);
        }
    }
}