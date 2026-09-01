using System.Net;

namespace Hall_rent.Exceptions.Handling;

public sealed record ExceptionResolution(
    IReadOnlyList<string> Errors,
    HttpStatusCode StatusCode,
    string Title,
    LogLevel LogLevel,
    Exception Exception);