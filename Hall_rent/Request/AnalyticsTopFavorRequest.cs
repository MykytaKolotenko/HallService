using Hall_rent.Request.Interface;

namespace Hall_rent.Request;

public record AnalyticsTopFavorRequest : IDateRange
{
    public int Limit { get; init; } = 10;
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}