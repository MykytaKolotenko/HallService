using System.Net;

namespace Hall_rent.Exceptions.Handling;

// The last resolver in the chain catches everything that the others didn’t recognize.
public class FallbackExceptionResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex)
    {
        return true;
    }

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        return new ExceptionResolution(["Internal Server Error"], HttpStatusCode.InternalServerError, "Internal Server Error", LogLevel.Error, ex);
    }
}
