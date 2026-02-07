namespace Nexus.Identity.API.Infrastructure.RateLimiting
{
    public interface IRateLimiterService
    {
        Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan timeWindow);
    }
}
