namespace Hall_rent.Request;

public record DateRangeRequest
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
};
