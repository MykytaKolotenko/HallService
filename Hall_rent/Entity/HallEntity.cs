namespace Hall_rent.Entity;

public class HallEntity
{
    public Guid Id { get; set; }
    public List<Guid> Favors { get; set; } = [];
    public int Persons { get; set; }
    public decimal Price { get; set; }
    public string Name { get; set; }

    public ICollection<HallFavorEntity> FavorsEntity { get; set; }
        = [];
}
