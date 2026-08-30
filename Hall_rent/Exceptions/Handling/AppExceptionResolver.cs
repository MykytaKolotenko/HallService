namespace Hall_rent.Exceptions.Handling;

public class AppExceptionResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex) => ex is AppException;

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        var appEx = (AppException)ex;
        return new ExceptionResolution([appEx.Message], appEx.StatusCode, appEx.Title, appEx.LogLevel, appEx);
    }
}