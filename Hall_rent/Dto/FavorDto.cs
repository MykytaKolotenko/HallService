namespace Hall_rent.Dto;

public record FavorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}