using System.Net;

namespace Hall_rent.Exceptions;

public class BookingConflictException : AppException
{
    public BookingConflictException(Guid hallId, Exception inner)
        : base($"Could not complete booking for hall {hallId} due to a concurrent update. Please retry.", inner)
    {
        HallId = hallId;
    }

    public Guid HallId { get; }

    public override HttpStatusCode StatusCode { get; } = HttpStatusCode.Conflict;
    public override LogLevel LogLevel => LogLevel.Warning;
}