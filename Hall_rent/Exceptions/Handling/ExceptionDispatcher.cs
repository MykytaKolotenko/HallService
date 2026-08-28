namespace Hall_rent.Exceptions.Handling;

public class ExceptionDispatcher
{
    private readonly IReadOnlyList<IExceptionHandler> _handlers;

    public ExceptionDispatcher(IReadOnlyList<IExceptionHandler> handlers)
    {
        _handlers = handlers;
    }

    public ExceptionResolution Resolve(Exception ex, string context = "") =>
        _handlers.First(h => h.CanHandle(ex)).Resolve(ex, context);
}