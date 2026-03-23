using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nexus.Identity.API.Constants;
using Nexus.Identity.API.Data;
using Nexus.Identity.API.Features.Registration;
using Nexus.Identity.API.Features.VerifyOtp;
using Nexus.Identity.API.Infrastructure.Extension;
using Nexus.Identity.API.Infrastructure.Middleware;
using Nexus.Identity.API.Infrastructure.RateLimiting;
using Nexus.Identity.API.Services;
using Nexus.Shared.Utilities;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Redis
builder.AddRedisClient("redis");

builder.Services.AddSingleton<IRateLimiterService, RedisRateLimiterService>();
builder.Services.AddSingleton<GlobalRateLimitPolicy>();
builder.Services.AddSingleton<RateLimitingPolicyProvider>();
builder.Services.AddScoped<OtpVerificationService>();

// Database
builder.AddNpgsqlDbContext<AppDbContext>("IdentityDb", settings =>
{
    settings.DisableRetry = false;
});

builder.Services.AddOpenApi();

// MediatR & FluentValidation
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

//SnowFlake ID Generator
builder.Services.AddSingleton<SnowFlakeIdGenerator>(
    _ => new SnowFlakeIdGenerator(ServerConstants.ServerId)
);

// MassTransit with RabbitMQ
builder.Services.AddMessageBroker(builder.Configuration);

var app = builder.Build();

// ADD THIS EXACT LINE RIGHT HERE:
app.UseForwardedHeaders();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Scalar API Reference
    app.MapScalarApiReference(options =>
    {
        options.AddServer(new ScalarServer("https://identity-api.lemonwater-41f24217.australiaeast.azurecontainerapps.io"));
    });
}

app.UseMiddleware<RateLimitingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.MapRegisterUserEndPoint();
app.MapVerifyOtpEndPoint();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
