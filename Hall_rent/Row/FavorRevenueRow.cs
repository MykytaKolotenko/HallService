namespace Hall_rent.Row;

public record FavorRevenueRow
{
    public Guid Id { get; init; }
    public int BookingsCount { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Revenue { get; init; }
}