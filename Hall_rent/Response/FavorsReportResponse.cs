namespace Hall_rent.Response;

public record FavorsReportResponse()
{
    public decimal Revenue { get; set; }
    public int BookingsCount { get; set; }
    public List<FavorRevenueResponse> Favors { get; set; } = [];
};
