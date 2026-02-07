using Nexus.Identity.API.Constants;

namespace Nexus.Identity.API.Infrastructure.RateLimiting
{
    public class RateLimitingPolicyProvider
    {
        private readonly Dictionary<string, RateLimitingPolicy> _policies;
        public RateLimitingPolicyProvider()
        {
            _policies = new Dictionary<string, RateLimitingPolicy>
            {
                [ApiEndpoints.Register] = new RateLimitingPolicy
                {
                    MaxRequests = 3,
                    TimeWindow = TimeSpan.FromMinutes(10),
                    UseEmail = true,
                    UseIp = true,
                    CooldownRequests = 1,
                    CooldownWindow = TimeSpan.FromSeconds(30)
                }
            };
        }

        public RateLimitingPolicy? Getpolicy(string Endpoint)
        {
            if(string.IsNullOrEmpty(Endpoint))
                return null;

            return _policies.TryGetValue(Endpoint, out var policy)
                ? policy
                : null;
        }
    }
}
