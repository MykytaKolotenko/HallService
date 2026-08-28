using System.Net;

namespace Hall_rent.Exceptions;

public class HallNameAlreadyExistsException : AppException
{
    public HallNameAlreadyExistsException(string name, Exception inner)
        : base($"Hall with name '{name}' already exists.", inner)
    {
        Name = name;
    }

    public string Name { get; }

    public override HttpStatusCode StatusCode { get; } = HttpStatusCode.Conflict;
    public override LogLevel LogLevel => LogLevel.Warning;
}
