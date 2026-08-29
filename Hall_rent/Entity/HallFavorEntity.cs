namespace Hall_rent.Entity;

public class HallFavorEntity
{
    public Guid HallId { get; set; }
    public HallEntity Hall { get; set; } = null!;

    public Guid FavorId { get; set; }
    public FavorEntity Favor { get; set; } = null!;
}