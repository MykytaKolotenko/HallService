namespace Hall_rent.Request;

public record FavorCreateRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}
