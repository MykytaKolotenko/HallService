namespace Hall_rent.Entity;

public class HallEntity
{
    public Guid Id { get; set; }
    public List<Guid> Favors { get; set; }
    public int Persons { get; set; }
    public decimal Price { get; set; }

    public HallEntity()
    {
    }

    public HallEntity(int persons, decimal price, List<Guid> favors)
    {
        Persons = persons;
        Price = price;
        Favors = favors;
    }
}
