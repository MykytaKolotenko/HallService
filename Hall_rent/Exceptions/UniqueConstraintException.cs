using System.Net;

namespace Hall_rent.Exceptions;

public sealed class UniqueConstraintException : AppException
{
    public UniqueConstraintException(string constraint, Exception inner)
        : base($"Unique constraint '{constraint}' was violated.", inner)
    {
        Constraint = constraint;
    }

    public string Constraint { get; }

    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    public override LogLevel LogLevel => LogLevel.Warning;
}
