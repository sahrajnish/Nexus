using StackExchange.Redis;

namespace Nexus.Identity.API.Infrastructure.Extension
{
    public static class RedisExtension
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            // Get the Redis connection string from Aspire configuration and configure the connection multiplexer
            var redisConnectionString = configuration.GetConnectionString("redis");

            if(string.IsNullOrEmpty(redisConnectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured.");
            }

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                return ConnectionMultiplexer.Connect(redisConnectionString!);
            });
            return services;
        }
    }
}
