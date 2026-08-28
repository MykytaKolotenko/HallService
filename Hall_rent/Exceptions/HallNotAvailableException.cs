using System.Net;

namespace Hall_rent.Exceptions;

public class HallNotAvailableException : AppException
{
    public HallNotAvailableException(Guid hallId, DateTime startAt, DateTime endAt)
        : base($"Hall {hallId} is not available from {startAt:yyyy-MM-dd HH:mm} to {endAt:yyyy-MM-dd HH:mm}.")
    {
        HallId = hallId;
    }

    public Guid HallId { get; }
    public override HttpStatusCode StatusCode => HttpStatusCode.Conflict;
}