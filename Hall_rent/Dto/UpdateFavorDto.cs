namespace Hall_rent.Dto;

public record UpdateFavorDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}