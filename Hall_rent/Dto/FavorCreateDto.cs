namespace Hall_rent.Dto;

public record FavorCreateDto()
{
    public string Name { get; init; }
    public decimal Price { get; init; }
};