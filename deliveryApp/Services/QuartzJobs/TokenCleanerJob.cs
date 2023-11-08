using deliveryApp.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace deliveryApp.Services.QuartzJobs
{
    public class TokenCleanerJob : IJob
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;
        public TokenCleanerJob(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Token cleaner job has been started yet again");
            var allTokens = await _context.Tokens.ToListAsync();
            foreach (var token in allTokens)
            {
                if (token.ExpirationDate <= DateTime.Now.ToUniversalTime())
                {
                    _logger.LogInformation($"[INFO][DateTimeUTC: {DateTime.UtcNow}]Token {token.Token} has been removed from the database due to it being expired");
                    _context.Remove(token);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
