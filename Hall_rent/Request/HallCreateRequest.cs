namespace Hall_rent.Request;

public struct HallCreateRequest
{
    public List<Guid> Favors { get; set; }
    public int Persons { get; set; }
    public decimal Price { get; set; }
}
