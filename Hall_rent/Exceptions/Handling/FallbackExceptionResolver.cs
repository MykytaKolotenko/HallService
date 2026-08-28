using System.Net;

namespace Hall_rent.Exceptions.Handling;

public class FallbackExceptionResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex) => true;

    public ExceptionResolution Resolve(Exception ex, string context) =>
        new(ex, HttpStatusCode.InternalServerError, "Internal Server Error", LogLevel.Error);
}