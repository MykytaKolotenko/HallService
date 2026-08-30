using System.Net;

namespace Hall_rent.Exceptions;

public class FavorsNotOfferedException : AppException
{
    public FavorsNotOfferedException(Guid hallId, IEnumerable<Guid> favourIds)
        : base($"Hall {hallId} does not offer favours: {string.Join(", ", favourIds)}")
    {
        HallId = hallId;
    }

    public Guid HallId { get; }
    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}