namespace Hall_rent.Entity;

public class HallBookingEntity
{
    public Guid Id { get; set; }
    public Guid HallId { get; set; }
    public decimal Price { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public ICollection<HallBookingFavorEntity> Favors { get; set; } = [];
}
