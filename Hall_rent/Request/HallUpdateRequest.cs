namespace Hall_rent.Request;

public record HallUpdateRequest
{
    public decimal Price { get; set; }
    public int Persons { get; set; }
    public List<Guid>? Favors { get; set; }
}
