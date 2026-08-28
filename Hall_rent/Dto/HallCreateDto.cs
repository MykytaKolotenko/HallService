namespace Hall_rent.Dto;

public struct HallCreateDto
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int Persons { get; private set; }
    public List<Guid>? Favors { get; private set; }

    public HallCreateDto(string name, decimal price, int persons, List<Guid>? favors)
    {
        Favors = favors ?? new List<Guid>();
        Name = name;
        Persons = persons;
        Price = price;
    }
}
