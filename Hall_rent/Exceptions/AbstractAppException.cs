using System.Net;

namespace Hall_rent.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message, Exception? inner = null) : base(message, inner)
    {
    }

    public abstract HttpStatusCode StatusCode { get; }

    public virtual string Title => GetType().Name.Replace("Exception", "");

    public virtual LogLevel LogLevel => LogLevel.Information;
}