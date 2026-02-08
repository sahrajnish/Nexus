using Nexus.Notification.API.Configurations;
using Nexus.Notification.API.Infrastructure.Extensions;
using Nexus.Notification.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add MassTransit and RabbitMQ configuration
builder.Services.AddMessageBroker(builder.Configuration);

// Add Mailjet configuration
builder.Services.Configure<MailjetOptions>(
    builder.Configuration.GetSection("Mailjet")
);

// Add application services
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
