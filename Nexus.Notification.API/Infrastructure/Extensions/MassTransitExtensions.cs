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
                    var host = configuration["MessageBroker:Host"];
                    var user = configuration["MessageBroker:Username"];
                    var pass = configuration["MessageBroker:Password"];

                    cfg.Host(host, "/", h =>
                    {
                        h.Username(user);
                        h.Password(pass);
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
