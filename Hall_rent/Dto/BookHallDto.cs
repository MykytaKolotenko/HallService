namespace Hall_rent.Dto;

public struct BookHallDto
{
    public Guid HallId { get; set; }
    public List<Guid> Favors { get; set; }

    public int Persons { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
