using deliveryApp.Models;
using deliveryApp.Models.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace deliveryApp.Policies
{
    public class AuthorizationPolicyHandler : AuthorizationHandler<AuthorizationPolicy>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AuthorizationPolicyHandler> _logger;
        public AuthorizationPolicyHandler(IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory, ILogger<AuthorizationPolicyHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizationPolicy requirement)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new Unauthorized("Can");
            }
            var token = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Authorization];
            await ValidateToken(token.First().Replace("Bearer ", ""));
            context.Succeed(requirement);
        }

        private async Task ValidateToken(string token)
        {
            var _context = _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
            var tokenInDB = await _context.Tokens.Where(x => token == x.Token).FirstOrDefaultAsync();
            if (tokenInDB == null)
            {
                _logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Token {token} has not been found in database");
                throw new Unauthorized($"The token does not exist in database (token: {token})");
            }
            else if (tokenInDB.ExpirationDate < DateTime.Now.ToUniversalTime())
            {
                _context.Tokens.Remove(tokenInDB);
                await _context.SaveChangesAsync();
                //_logger.LogError($"[ERROR][DateTimeUTC: {DateTime.UtcNow}]Token {token} has expired");
                throw new Forbidden($"Token is expired (token: {token})");
            }
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Token {token} has been validated");
        }
    }
}
