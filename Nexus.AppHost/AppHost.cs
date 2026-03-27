var builder = DistributedApplication.CreateBuilder(args);

// Add RabbitMQ
var rabbit = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin(); // Enables RabbitMQ management plugin for easier monitoring and management.

// Add PostgreSQL
var postgres = builder.AddPostgres("Nexus")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

// Add a database for Identity API. This will create a PostgreSQL database named "IdentityDb" that the Identity API can use for its data storage.
var identityDb = postgres.AddDatabase("IdentityDb");

var videoDb = postgres.AddDatabase("VideoDb");

// Add Redis for caching and rate limiting.
var redis = builder.AddRedis("redis")
    .WithDataVolume() // Data persists even if container restarts.
    .WithLifetime(ContainerLifetime.Persistent); // Redis Container stays alive across Aspire resta rts.

// Add Azure Storage and configure it to use the local Azurite emulator
//var storage = builder.AddAzureStorage("nexus-storage")
//    .RunAsEmulator();

//// Define a Blob Storage endpoint specifically for video files
//var rawVideoBlobs = storage.AddBlobs("raw-videos");
//var processingVideoBlob = storage.AddBlobs("processing-videos");

// Add the Identity API project and reference the IdentityDb, Redis, and RabbitMQ services.
builder.AddProject<Projects.Nexus_Identity_API>("nexus-identity-api")
    .WithReference(identityDb)
    .WithReference(redis)
    .WithReference(rabbit);

// Add the Notification API project and reference RabbitMQ for message handling.
builder.AddProject<Projects.Nexus_Notification_API>("nexus-notification-api")
    .WithReference(rabbit);

// Add the Video API project
builder.AddProject<Projects.Nexus_Video_API>("nexus-video-api")
    .WithReference(videoDb);

builder.Build().Run();
