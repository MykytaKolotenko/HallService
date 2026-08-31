namespace Hall_rent.Dto;

public record BookHallDto
{
    public Guid HallId { get; init; }
    public List<Guid> Favors { get; init; } = [];

    public int Persons { get; init; }

    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
}