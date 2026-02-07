using Nexus.Identity.API.Infrastructure.RateLimiting;
using System.Text.Json;

namespace Nexus.Identity.API.Infrastructure.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RateLimitingPolicyProvider policyProvider, IRateLimiterService rateLimiterService, GlobalRateLimitPolicy globalPolicy)
        {
            var endpoint = context.Request.Path.Value;
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                await _next(context);
                return;
            }

            if (endpoint.StartsWith("/health", StringComparison.OrdinalIgnoreCase) || 
                endpoint.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase) || 
                endpoint.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            // Global Cooldown check.
            var globalCooldownKey = "rl:global:cooldown";

            var globalCooldownAllowed = await rateLimiterService.IsAllowedAsync(globalCooldownKey, globalPolicy.CooldownRequests, globalPolicy.CooldownWindow);
            if(!globalCooldownAllowed)
            {
                _logger.LogWarning("Cooldown exceeded for global server.");
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = globalPolicy.CooldownWindow.TotalSeconds.ToString();
                await context.Response.WriteAsync("Server overloaded.");
                return;
            }

            // Global Rate Limit
            var globalKey = "rl:global";

            var globalAllowed = await rateLimiterService.IsAllowedAsync(globalKey, globalPolicy.MaxRequests, globalPolicy.TimeWindow);
            if (!globalAllowed)
            {
                _logger.LogWarning("Rate Limit exceeded for global server.");
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers["Retry-After"] = globalPolicy.TimeWindow.TotalSeconds.ToString();
                await context.Response.WriteAsync("Server Busy. Try again later.");
                return;
            }

            var policy = policyProvider.Getpolicy(endpoint);
            if(policy == null)
            {
                await _next(context);
                return;
            }

            var ipAddress = context.Request.Headers["X-Forwarded-For"]
                    .FirstOrDefault()
                    ?.Split(',')[0]
                    .Trim()
                ?? context.Connection.RemoteIpAddress?.ToString();

            string? email = null;

            if(policy.UseEmail)
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, leaveOpen: true);

                var body = await reader.ReadToEndAsync();

                context.Request.Body.Position = 0;

                try
                {
                    var json = JsonDocument.Parse(body);
                    email = json.RootElement
                        .GetProperty("email")
                        .GetString()
                        ?.ToLowerInvariant();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse email from request body for rate limiting.");
                }
            }

            // Cooldown for Email. To prevent burst on API.
            if (policy.UseEmail && !string.IsNullOrEmpty(email) && policy.CooldownRequests > 0)
            {
                var cooldownKey = $"rl:{endpoint}:cooldown-email:{email}";

                var cooldownAllowed = await rateLimiterService.IsAllowedAsync(cooldownKey, policy.CooldownRequests, policy.CooldownWindow);
                if (!cooldownAllowed)
                {
                    _logger.LogWarning("Cooldown exceeded for email {Email} on endpoint {Endpoint}", email, endpoint);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = policy.CooldownWindow.TotalSeconds.ToString();
                    await context.Response.WriteAsync("Please wait before requesting again.");
                    return;
                }
            }

            // Rate limit by Email
            if (policy.UseEmail && !string.IsNullOrEmpty(email))
            {
                var emailKey = $"rl:{endpoint}:email:{email}";

                var allowed = await rateLimiterService.IsAllowedAsync(emailKey, policy.MaxRequests, policy.TimeWindow);
                if (!allowed)
                {
                    _logger.LogWarning("Rate limit exceeded for email {Email} on endpoint {Endpoint}", email, endpoint);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = policy.TimeWindow.TotalSeconds.ToString();
                    await context.Response.WriteAsync("Too many requests. Please try again later.");
                    return;
                }
            }

            // Cooldown for IP. To prevent burst on API.
            if (policy.UseIp && !string.IsNullOrWhiteSpace(ipAddress) && ipAddress != "unknown" && policy.CooldownRequests > 0)
            {
                var cooldownIpKey = $"rl:{endpoint}:cooldown-ip:{ipAddress}";

                var allowed = await rateLimiterService.IsAllowedAsync(cooldownIpKey, policy.CooldownRequests, policy.CooldownWindow);
                if (!allowed)
                {
                    _logger.LogWarning("Cooldown exceeded for IP {IP} on endpoint {Endpoint}", ipAddress, endpoint);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = policy.CooldownWindow.TotalSeconds.ToString();
                    await context.Response.WriteAsync("Too many requests. Please try again later.");
                    return;
                }
            }

            // Rate limit by IP
            if (policy.UseIp && !string.IsNullOrWhiteSpace(ipAddress) && ipAddress != "unknown")
            {
                var ipKey = $"rl:{endpoint}:ip:{ipAddress}";

                var allowed = await rateLimiterService.IsAllowedAsync(ipKey, policy.MaxRequests, policy.TimeWindow);
                if(!allowed)
                {
                    _logger.LogWarning("Rate limit exceeded for IP {IP} on endpoint {Endpoint}", ipAddress, endpoint);
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = policy.TimeWindow.TotalSeconds.ToString();
                    await context.Response.WriteAsync("Too many requests. Please try again later.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
