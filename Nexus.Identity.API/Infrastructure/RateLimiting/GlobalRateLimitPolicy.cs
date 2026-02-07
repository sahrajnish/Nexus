namespace Nexus.Identity.API.Infrastructure.RateLimiting
{
    public class GlobalRateLimitPolicy
    {
        public int MaxRequests { get; set; } = 2000;
        public TimeSpan TimeWindow { get; set; } = TimeSpan.FromSeconds(1);
        public int CooldownRequests { get; set; } = 200;
        public TimeSpan CooldownWindow { get; set; } = TimeSpan.FromSeconds(1);
    }
}
