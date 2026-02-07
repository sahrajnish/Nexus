
using StackExchange.Redis;

namespace Nexus.Identity.API.Infrastructure.RateLimiting
{
    public class RedisRateLimiterService : IRateLimiterService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisRateLimiterService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _database = _redis.GetDatabase();
        }

        public async Task<bool> IsAllowedAsync(string key, int maxRequests, TimeSpan timeWindow)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Calculate earliest allowed timestamp; requests older than this fall outside the window.
            var windowStart = now - (long)timeWindow.TotalMilliseconds;

            // Use Redis transaction to ensure atomic remove, add, count, and expire operations.
            var transaction = _database.CreateTransaction();

            // Remove requests older than sliding window to keep only valid recent entries.
            _ = transaction.SortedSetRemoveRangeByScoreAsync(key, 0, windowStart);

            // Add current request with unique member and timestamp score.
            _ = transaction.SortedSetAddAsync(key, Guid.NewGuid().ToString(), now);

            var countTask = transaction.SortedSetLengthAsync(key);

            // Set expiration so Redis auto-cleans inactive keys and prevents memory growth.
            _ = transaction.KeyExpireAsync(key, timeWindow);

            await transaction.ExecuteAsync();

            var requestCount = await countTask;

            return requestCount <= maxRequests;
        }
    }
}
