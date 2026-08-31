namespace Hall_rent.Request;

public record HallUpdateRequest
{
    public decimal Price { get; init; }
    public int Persons { get; init; }

    public string Name { get; init; }
    public List<Guid>? Favors { get; init; }
}