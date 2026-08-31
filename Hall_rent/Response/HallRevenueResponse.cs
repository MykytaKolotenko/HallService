namespace Hall_rent.Response;

public record HallRevenueResponse()
{
    public DateTime Day { get; set; }
    public decimal Revenue { get; set; }
    public int BookingsCount { get; set; }
};
