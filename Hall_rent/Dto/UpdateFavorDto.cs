namespace Hall_rent.Dto;

public struct UpdateFavorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
