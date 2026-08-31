namespace Hall_rent.Request;

public record FavorUpdateRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
