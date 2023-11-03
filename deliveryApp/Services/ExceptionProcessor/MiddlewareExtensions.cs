namespace deliveryApp.Services.ExceptionProcessor
{
    public static class MiddlewareExtensions
    {
        public static void UseExceptionMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionMiddlewareService>();
        }
    }
}
