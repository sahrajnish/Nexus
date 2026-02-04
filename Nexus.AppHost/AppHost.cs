var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("Nexus")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent); 

var identityDb = postgres.AddDatabase("IdentityDb");

builder.AddProject<Projects.Nexus_Identity_API>("nexus-identity-api")
    .WithReference(identityDb);

builder.Build().Run();
