using System.Text.Json;
using Hall_rent.Exceptions.Handling;

namespace Hall_rent.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ExceptionDispatcher exceptionDispatcher)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var resolution = exceptionDispatcher.Resolve(ex, $"{context.Request.Method} {context.Request.Path}");

            _logger.Log(resolution.LogLevel, resolution.Exception,
                "{Title} on {Method} {Path}", resolution.Title, context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)resolution.StatusCode;

            var problem = new
            {
                title = resolution.Title,
                status = (int)resolution.StatusCode,
                detail = resolution.Exception.Message,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
