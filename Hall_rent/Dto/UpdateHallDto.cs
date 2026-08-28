namespace Hall_rent.Dto;

public struct UpdateHallDto
{
    public Guid Id { get; private set; }
    public decimal Price { get; private set; }
    public int Persons { get; private set; }
    public List<Guid>? Favors { get; private set; }

    public UpdateHallDto(Guid id, decimal price, int persons, List<Guid>? favors)
    {
        Id = id;
        Price = price;
        Persons = persons;
        Favors = favors ?? new List<Guid>();
    }
}