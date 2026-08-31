using Hall_rent.Request.Interface;

namespace Hall_rent.Request;

public record HallSearchRequest : IDateRange
{
    public int Persons { get; init; }
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}