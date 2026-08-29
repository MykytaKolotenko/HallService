using System.Net;
using Hall_rent.Exceptions;

public sealed class HallCapacityExceededException : AppException
{
    public HallCapacityExceededException(Guid hallId, int capacity, int requested)
        : base(
            $"Hall {hallId} has capacity {capacity}, but {requested} persons were requested.")
    {
        HallId = hallId;
        Capacity = capacity;
        Requested = requested;
    }

    public Guid HallId { get; }
    public int Capacity { get; }
    public int Requested { get; }

    public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;
}