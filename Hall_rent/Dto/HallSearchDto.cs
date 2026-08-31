namespace Hall_rent.Dto;

public record HallSearchDto
{
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public int Persons { get; init; }
}