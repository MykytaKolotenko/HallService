namespace Hall_rent.Dto;

public struct UpdateFavorDto
{
    public Guid Id;
    public string Name;
    public decimal Price;

    public UpdateFavorDto(Guid id, string name, decimal price)
    {
        Id = id;
        Name = name;
        Price = price;
    }
}