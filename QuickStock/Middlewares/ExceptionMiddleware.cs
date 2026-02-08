using Microsoft.AspNetCore.Diagnostics;
using QuickStock.Common.Exceptions;

namespace QuickStock.Middlewares
{
    // ✅ Static class (non-generic)
    public static class ExceptionMiddlewareExtensions
    {
        // ✅ Static method with 'this WebApplication app'
        public static void ConfigureCustomExceptionMiddleware(this WebApplication app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                    if (exception == null)
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsJsonAsync(new { message = "Unknown error occurred." });
                        return;
                    }

                    switch (exception)
                    {
                        case BadRequestException ex:
                            context.Response.StatusCode = 400;
                            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
                            break;
                        case UnauthorizedException ex:
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
                            break;
                        case NotFoundException ex:
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsJsonAsync(new { message = ex.Message });
                            break;
                        default:
                            context.Response.StatusCode = 500;
                            await context.Response.WriteAsJsonAsync(new
                            {
                                message = "An unexpected error occurred.",
                                details = exception.Message
                            });
                            break;
                    }
                });
            });
        }
    }
}
