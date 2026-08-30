using Hall_rent.Exceptions.Handling;

namespace Hall_rent.Middleware;

public sealed class CustomExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public CustomExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // ExceptionDispatcher и ILogger — scoped/transient-совместимые сервисы, поэтому получаем их
    // через параметры InvokeAsync (middleware сам по себе singleton), а не через конструктор.
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
            ExceptionResolution resolution = dispatcher.Resolve(ex, context.Request.Path);

            logger.Log(
                resolution.LogLevel,
                resolution.Exception,
                "Unhandled exception while processing {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            if (context.Response.HasStarted)
            {
                // Тело ответа уже начало отправляться (например, стриминг) — статус/тело не переписать.
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
