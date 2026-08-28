using System.Net;
using Hall_rent.Exceptions;

public class ConcurrencyConflictException : AppException
{
    public ConcurrencyConflictException(string context, Exception inner)
        : base($"Could not complete '{context}' due to a concurrent update. Please retry.", inner)
    {
    }

    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    public override LogLevel LogLevel => LogLevel.Warning;
}