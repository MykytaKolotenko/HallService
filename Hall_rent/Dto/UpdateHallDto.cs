namespace Hall_rent.Dto;

public struct UpdateHallDto
{
    public Guid Id;
    public decimal Price;
    public int Persons;
    public List<Guid> Favors;

    public UpdateHallDto(Guid id, decimal price, int persons, List<Guid> favors)
    {
        Id = id;
        Price = price;
        Persons = persons;
        Favors = favors;
    }
}