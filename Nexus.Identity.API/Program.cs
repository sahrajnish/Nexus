using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
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

// Trust Azure Container Apps reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Clear the known networks/proxies so it trusts the dynamic ACA load balancer
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

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
    app.MapScalarApiReference();
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
