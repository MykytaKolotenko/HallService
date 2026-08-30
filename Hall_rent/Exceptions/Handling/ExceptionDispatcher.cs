using System.Net;

namespace Hall_rent.Exceptions.Handling;

public class ExceptionDispatcher
{
    private readonly IReadOnlyList<IExceptionResolver> _handlers;

    public ExceptionDispatcher(IReadOnlyList<IExceptionResolver> handlers)
    {
        _handlers = handlers;
    }

    public ExceptionResolution Resolve(Exception ex, string context = "")
    {
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(ex));

        return handler?.Resolve(ex, context)
               ?? new ExceptionResolution(
                   ["Internal Server Error"],
                   HttpStatusCode.InternalServerError,
                   "Internal Server Error",
                   LogLevel.Error,
                   ex);
    }
}
