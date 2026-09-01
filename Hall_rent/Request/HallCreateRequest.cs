namespace Hall_rent.Request;

public record HallCreateRequest
{
    public List<Guid>? Favors { get; init; }

    public string Name { get; init; } = string.Empty;
    public int Persons { get; init; }
    public decimal Price { get; init; }
}