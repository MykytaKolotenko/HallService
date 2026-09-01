namespace Hall_rent.Dto;

public record FavorCreateDto()
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
};