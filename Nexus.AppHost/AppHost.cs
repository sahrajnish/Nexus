var builder = DistributedApplication.CreateBuilder(args);

var rabbit = builder.AddRabbitMQ("messaging")
    .WithManagementPlugin();

var postgres = builder.AddPostgres("Nexus")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent); 

var identityDb = postgres.AddDatabase("IdentityDb");

builder.AddProject<Projects.Nexus_Identity_API>("nexus-identity-api")
    .WithReference(identityDb)
    .WithReference(rabbit);

builder.AddProject<Projects.Nexus_Notification_API>("nexus-notification-api");

builder.Build().Run();
