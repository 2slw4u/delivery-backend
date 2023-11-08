using deliveryApp.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace deliveryApp.Services.QuartzJobs
{
    public class TokenCleanerJob : IJob
    {
        private readonly AppDbContext _context;
        public TokenCleanerJob(AppDbContext context)
        {
            _context = context;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var allTokens = await _context.Tokens.ToListAsync();
            foreach (var token in allTokens)
            {
                if (token.ExpirationDate <= DateTime.Now.ToUniversalTime())
                {
                    _context.Remove(token);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}
