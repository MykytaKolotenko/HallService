using System.Net;

namespace Hall_rent.Exceptions.Handling;

public class FallbackExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception ex) => true;

    public ExceptionResolution Resolve(Exception ex, string context) =>
        new(ex, HttpStatusCode.InternalServerError, "Internal Server Error", LogLevel.Error);
}