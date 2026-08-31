namespace Hall_rent.Response;

public record FavorRevenueResponse()
{
    public Guid Id { get; set; }
    public int BookingsCount { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
};
