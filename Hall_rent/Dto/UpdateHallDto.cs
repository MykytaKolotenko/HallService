namespace Hall_rent.Dto;

public struct UpdateHallDto
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Persons { get; set; }
    public List<Guid>? Favors { get; set; }
}
