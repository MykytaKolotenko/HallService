using Hall_rent.Request.Interface;

namespace Hall_rent.Request;

public record HallUpdateRequest : IHallRequest
{
    public List<Guid>? Favors { get; init; }
    public decimal Price { get; init; }
    public int Persons { get; init; }

    public string Name { get; init; } = string.Empty;
}