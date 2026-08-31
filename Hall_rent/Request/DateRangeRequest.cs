namespace Hall_rent.Request;

public record DateRangeRequest
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
};