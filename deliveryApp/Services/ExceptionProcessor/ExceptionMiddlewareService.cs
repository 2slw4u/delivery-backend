using deliveryApp.Models.DTOs;
using deliveryApp.Models.Exceptions;

namespace deliveryApp.Services.ExceptionProcessor
{
    public class ExceptionMiddlewareService
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddlewareService> _logger;
        public ExceptionMiddlewareService(RequestDelegate next, ILogger<ExceptionMiddlewareService> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFound ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsJsonAsync(new { message = "404Not Found: " + ex.Message });
            }
            catch (Forbidden ex)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "403Forbidden: " + ex.Message });
            }
            catch (Conflict ex)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(new { message = "409Conflict: " + ex.Message });
            }
            catch (BadRequest ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(new { message = "400BadRequst: " + ex.Message });
            }
            catch (Unauthorized ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "401Unauthorized: " + ex.Message });
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                _logger.LogError(ex.Message);
                await context.Response.WriteAsJsonAsync(new { message = "500Internal Server Error, something went undeniably wrong"});
            }
        }
    }
}
