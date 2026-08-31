namespace Hall_rent.Response;

public record HallBookResponse
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
}
