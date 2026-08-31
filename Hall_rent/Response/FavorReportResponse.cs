namespace Hall_rent.Response;

public record FavorReportResponse()
{
    public decimal Revenue { get; set; }
    public int BookingsCount { get; set; }
    public List<FavorRevenueResponse> Favors { get; set; } = [];
};