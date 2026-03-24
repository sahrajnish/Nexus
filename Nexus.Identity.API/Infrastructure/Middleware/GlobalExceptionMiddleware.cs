using Nexus.Identity.API.Domain.Exceptions;

namespace Nexus.Identity.API.Infrastructure.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                context.Response.StatusCode = ex switch
                {
                    ConflictException => StatusCodes.Status409Conflict,
                    NotFoundException => StatusCodes.Status404NotFound,
                    _ => StatusCodes.Status400BadRequest
                };

                await context.Response.WriteAsJsonAsync(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An Unhandled Exception Occurred: {ex.Message}");

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = $"An unexpected error occured: {ex.Message}"
                });
            }
        }
    }
}
