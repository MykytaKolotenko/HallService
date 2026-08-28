namespace Hall_rent.Response;

public struct FavorResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    public FavorResponse(Guid id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}