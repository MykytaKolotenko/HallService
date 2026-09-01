namespace Hall_rent.Dto;

public record HallCreateDto
{
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Persons { get; init; }
    public List<Guid>? Favors { get; init; }
}