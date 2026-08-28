namespace Hall_rent.Dto;

public struct UpdateFavorDto
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public UpdateFavorDto(Guid id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}