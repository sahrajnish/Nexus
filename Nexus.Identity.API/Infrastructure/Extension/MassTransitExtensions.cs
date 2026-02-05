using MassTransit;
using Nexus.Identity.API.Data;

namespace Nexus.Identity.API.Infrastructure.Extension
{
    public static class MassTransitExtensions
    {
        public static IServiceCollection AddMessageBroker (this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                // Create the Outbox for storing messages.
                x.AddEntityFrameworkOutbox<AppDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                // RabbitMQ queues to get the messages from outbox.
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
                    
                    // Scans for the consumers
                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
