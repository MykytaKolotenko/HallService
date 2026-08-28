namespace Hall_rent.Entity;

public class FavorEntity
{
    public FavorEntity()
    {
    }

    public FavorEntity(string name, decimal price)
    {
        Name = name;
        Price = price;
    }

    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}