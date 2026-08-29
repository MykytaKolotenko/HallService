namespace Hall_rent.Dto;

public record UpdateHallDto
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Persons { get; set; }
    public List<Guid>? Favors { get; set; }
}