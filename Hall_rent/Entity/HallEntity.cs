namespace Hall_rent.Entity;

public class HallEntity
{
    public HallEntity()
    {
    }

    public HallEntity(int persons, decimal price, List<Guid> favors, string name)
    {
        Persons = persons;
        Price = price;
        Favors = favors;
        Name = name;
    }

    public Guid Id { get; set; }
    public List<Guid> Favors { get; set; }
    public int Persons { get; set; }
    public decimal Price { get; set; }

    public string Name { get; set; }
}