using Hall_rent.Exceptions.Handling;

namespace Hall_rent.Middleware;

public sealed class CustomExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public CustomExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // ExceptionDispatcher and ILogger are scoped/transient-compatible services,
    // so we obtain them through InvokeAsync parameters (since the middleware itself is a singleton),
    // rather than through the constructor.
    public async Task InvokeAsync(
        HttpContext context,
        ExceptionDispatcher dispatcher,
        ILogger<CustomExceptionHandlingMiddleware> logger)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var resolution = dispatcher.Resolve(ex, context.Request.Path);

            logger.Log(
                resolution.LogLevel,
                resolution.Exception,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                logger.LogWarning(
                    "Response already started, cannot write error body for {Path}",
                    context.Request.Path);
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = (int)resolution.StatusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                title = resolution.Title,
                status = (int)resolution.StatusCode,
                errors = resolution.Errors,
                traceId = context.TraceIdentifier
            });
        }
    }
}