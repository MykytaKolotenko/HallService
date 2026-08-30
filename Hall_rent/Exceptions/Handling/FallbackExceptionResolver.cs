using System.Net;

namespace Hall_rent.Exceptions.Handling;

// Последний резолвер в цепочке — ловит вообще всё, что не распознали остальные.
public class FallbackExceptionResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex) => true;

    public ExceptionResolution Resolve(Exception ex, string context) =>
        new(["Internal Server Error"], HttpStatusCode.InternalServerError, "Internal Server Error", LogLevel.Error, ex);
}