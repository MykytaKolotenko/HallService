namespace Hall_rent.Exceptions.Handling;

public abstract class AppExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception ex) => ex is AppException;

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        var appEx = (AppException)ex;
        return new ExceptionResolution(appEx, appEx.StatusCode, appEx.Title, appEx.LogLevel);
    }
}