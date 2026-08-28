namespace Hall_rent.Exceptions.Handling;

public interface IExceptionResolver
{
    bool CanHandle(Exception ex);
    ExceptionResolution Resolve(Exception ex, string context);
}