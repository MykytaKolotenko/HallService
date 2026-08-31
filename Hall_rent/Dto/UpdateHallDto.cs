namespace Hall_rent.Dto;

public record UpdateHallDto
{
    public Guid Id { get; init; }
    public decimal Price { get; init; }
    public int Persons { get; init; }
    public List<Guid> Favors { get; init; } = [];

    public string Name { get; init; }
}