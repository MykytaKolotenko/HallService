namespace Hall_rent.Request;

public record HallBookRequest
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public List<Guid> Favors { get; set; }
    public int Persons { get; set; }
}
