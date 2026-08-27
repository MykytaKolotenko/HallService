namespace Hall_rent.Dto;

public struct BookHallDto
{
    public Guid HallId { get; private set; }
    public List<Guid> Favors { get; private set; }

    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }

    public BookHallDto(DateTime startAt, DateTime endAt, List<Guid> favors, Guid hallId)
    {
        StartAt = startAt;
        EndAt = endAt;
        Favors = favors;
        HallId = hallId;
    }
}
