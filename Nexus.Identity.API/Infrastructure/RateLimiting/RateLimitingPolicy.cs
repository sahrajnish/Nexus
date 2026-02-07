namespace Nexus.Identity.API.Infrastructure.RateLimiting
{
    public class RateLimitingPolicy
    {
        public int MaxRequests { get; init; }
        public TimeSpan TimeWindow { get; init; }
        public bool UseEmail { get; init; }
        public bool UseIp { get; init; }

        // Cooldown Settings
        public int CooldownRequests { get; init; }
        public TimeSpan CooldownWindow { get; init; }
    }
}
