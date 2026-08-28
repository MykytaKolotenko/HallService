namespace Hall_rent.Exceptions.Handling;

public interface IExceptionHandler
{
    bool CanHandle(Exception ex);
    ExceptionResolution Resolve(Exception ex, string context);
}