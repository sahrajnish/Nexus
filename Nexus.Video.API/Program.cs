using Amazon.Runtime;
using Amazon.S3;
using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Nexus.Shared.Utilities;
using Nexus.Video.API.Constants;
using Nexus.Video.API.Data;
using Nexus.Video.API.Infrastructure.Extension;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

// Database
builder.AddNpgsqlDbContext<AppDbContext>("VideoDb", settings =>
{
    settings.DisableRetry = false;
});

// Add RabbitMQ MassTransit
builder.Services.AddMessageBroker(builder.Configuration);

var s3config = new AmazonS3Config
{
    // The AWS SDK will often throw a null exception if no region is set, 
    // even when using a custom ServiceURL. We provide a dummy one here.
    AuthenticationRegion = "us-east-1"
};

AWSCredentials credentials;

// Local Development with MinIO
credentials = new BasicAWSCredentials("localminio", "localminio123");
s3config.ServiceURL = "http://localhost:9000";

s3config.ForcePathStyle = true;
s3config.UseHttp = true;

// Production with Cloudflare R2
//var accountId = builder.Configuration["CloudflareR2:AccountId"];
//var accessKey = builder.Configuration["CloudflareR2:AccessKey"];
//var secretKey = builder.Configuration["CloudflareR2:SecretKey"];

//credentials = new BasicAWSCredentials(accessKey, secretKey);
//s3config.ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com";

builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, s3config));

//builder.Services.AddOpenApi();

// Scalar Api Document Transformer -> Force it to https
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Nexus.Video.API | v2 (HTTPS FIXED)";

        document.Servers = new List<OpenApiServer>
        {
            new OpenApiServer { Url = "https://video-api.lemonwater-41f24217.australiaeast.azurecontainerapps.io" }
        };
        return Task.CompletedTask;
    });
});

// MediatR & FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

//SnowFlake ID Generator
builder.Services.AddSingleton<SnowFlakeIdGenerator>(
    _ => new SnowFlakeIdGenerator(ServerConstants.ServerId)
);

builder.Services.AddCarter();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// app.UseHttpsRedirection();

app.MapCarter();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
