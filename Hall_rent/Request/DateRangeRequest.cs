using Hall_rent.Request.Interface;

namespace Hall_rent.Request;

public record DateRangeRequest : IDateRange
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
};