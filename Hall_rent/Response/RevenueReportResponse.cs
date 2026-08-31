namespace Hall_rent.Response;

public record RevenueReportResponse
{
    public decimal TotalRevenue { get; init; }
    public int TotalBookings { get; init; }
    public List<HallRevenueResponse> RevenuePerDay { get; init; }
}
