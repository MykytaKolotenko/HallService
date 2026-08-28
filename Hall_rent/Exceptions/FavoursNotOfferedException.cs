using System.Net;

namespace Hall_rent.Exceptions;

public class FavoursNotOfferedException : AppException
{
    public FavoursNotOfferedException(Guid hallId, IEnumerable<Guid> favourIds)
        : base($"Hall {hallId} does not offer favours: {string.Join(", ", favourIds)}")
    {
        HallId = hallId;
    }

    public Guid HallId { get; }
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}