namespace Hall_rent.Request;

public record HallCreateRequest
{
    public List<Guid>? Favors { get; set; }

    public string Name { get; set; }
    public int Persons { get; set; }
    public decimal Price { get; set; }
}
