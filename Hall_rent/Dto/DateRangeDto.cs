namespace Hall_rent.Dto;

public record DateRangeDto
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}