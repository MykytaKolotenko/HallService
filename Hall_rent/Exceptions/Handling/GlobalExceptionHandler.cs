using Microsoft.AspNetCore.Diagnostics;

namespace Hall_rent.Exceptions.Handling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ExceptionDispatcher _dispatcher;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ExceptionDispatcher dispatcher,
        ILogger<GlobalExceptionHandler> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var resolution = _dispatcher.Resolve(
            exception,
            httpContext.Request.Path);

        _logger.Log(
            resolution.LogLevel,
            exception,
            "Unhandled application exception for {Path}",
            httpContext.Request.Path);

        httpContext.Response.StatusCode = (int)resolution.StatusCode;

        var response = new
        {
            title = resolution.Title,
            status = (int)resolution.StatusCode,
            errors = new[]
            {
                resolution.Exception.Message
            },
            traceId = httpContext.TraceIdentifier
        };

        await httpContext.Response.WriteAsJsonAsync(
            response,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);

        return true;
    }
}