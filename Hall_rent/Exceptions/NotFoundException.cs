using System.Net;

namespace Hall_rent.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public override HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}