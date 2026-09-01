using Hall_rent.Request.Interface;

namespace Hall_rent.Request;

public record HallBookRequest : IDateRange
{
    public List<Guid> Favors { get; init; } = [];
    public int Persons { get; init; }
    public DateTime From { get; init; }
    public DateTime To { get; init; }
}