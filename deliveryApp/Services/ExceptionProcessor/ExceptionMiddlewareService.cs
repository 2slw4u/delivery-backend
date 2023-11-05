using deliveryApp.Models.DTOs;
using deliveryApp.Models.Exceptions;

namespace deliveryApp.Services.ExceptionProcessor
{
    public class ExceptionMiddlewareService
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddlewareService(RequestDelegate next)
        {
            _next = next;
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
                await context.Response.WriteAsJsonAsync(ex);
            }
            catch (Forbidden ex)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(ex);
            }
            catch (Conflict ex)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                await context.Response.WriteAsJsonAsync(ex);
            }
            catch (BadRequest ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(ex);
            }
            catch (Unauthorized ex)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(ex);
            }
            catch (Exception ex)
            {
                var result = new Response()
                {
                    Status = "500",
                    Message = ex.Message
                };
                await context.Response.WriteAsJsonAsync(result);
            }
        }
    }
}
