namespace Hall_rent.Entity;

public class FavorEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public ICollection<HallFavorEntity> Halls { get; set; }
        = [];
}