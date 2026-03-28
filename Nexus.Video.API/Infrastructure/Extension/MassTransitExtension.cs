using MassTransit;
using Nexus.Video.API.Data;

namespace Nexus.Video.API.Infrastructure.Extension
{
    public static class MassTransitExtension
    {
        public static IServiceCollection AddMessageBroker (this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<AppDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                    o.DuplicateDetectionWindow = TimeSpan.FromMinutes(30);
                });

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

                    cfg.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
