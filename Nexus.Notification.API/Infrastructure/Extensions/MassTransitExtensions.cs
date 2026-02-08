using MassTransit;
using Nexus.Notification.API.Consumers;

namespace Nexus.Notification.API.Infrastructure.Extensions
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<UserRegisteredConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    var connectionString = configuration.GetConnectionString("rabbitmq");
                    if(string.IsNullOrEmpty(connectionString))
                    {
                        throw new InvalidOperationException("RabbitMQ connection string is not configured.");
                    }

                    var uri = new Uri(connectionString);

                    cfg.Host(uri, h =>
                    {
                        if(!string.IsNullOrEmpty(uri.UserInfo))
                        {
                            var parts = uri.UserInfo.Split(':', 2);
                            h.Username(parts[0]);

                            if (parts.Length > 1)
                                h.Password(parts[1]);
                        }
                    });

                    cfg.UseMessageRetry(r =>
                    {
                        r.Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromSeconds(1),
                            maxInterval: TimeSpan.FromSeconds(30),
                            intervalDelta: TimeSpan.FromSeconds(5));
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
