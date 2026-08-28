using Microsoft.AspNetCore.Diagnostics;

namespace Hall_rent.Exceptions.Handling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GlobalExceptionHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<ExceptionDispatcher>();

        var resolution = dispatcher.Resolve(exception, httpContext.Request.Path);

        httpContext.Response.StatusCode = (int)resolution.StatusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(new
        {
            title = resolution.Title,
            status = (int)resolution.StatusCode,
            errors = new[] { resolution.Exception.Message },
            traceId = httpContext.TraceIdentifier
        }, cancellationToken);

        return true;
    }
}
