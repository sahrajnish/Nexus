var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var postgres = builder.AddPostgres("Nexus")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent); 

var identityDb = postgres.AddDatabase("IdentityDb");

var redis = builder.AddRedis("redis")
    .WithDataVolume() // Data persists even if container restarts.
    .WithLifetime(ContainerLifetime.Persistent); // Redis Container stays alive across Aspire restarts.

builder.AddProject<Projects.Nexus_Identity_API>("nexus-identity-api")
    .WithReference(identityDb)
    .WithReference(redis)
    .WithReference(rabbit);

builder.AddProject<Projects.Nexus_Notification_API>("nexus-notification-api");

builder.Build().Run();
