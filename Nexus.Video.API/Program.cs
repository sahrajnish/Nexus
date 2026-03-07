using Carter;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nexus.Shared.Utilities;
using Nexus.Video.API.Constants;
using Nexus.Video.API.Data;
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

builder.Services.AddOpenApi();

builder.AddAzureBlobServiceClient("raw-videos");

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

app.UseHttpsRedirection();

app.MapCarter();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
