namespace Hall_rent.Entity;

public class HallBookingEntity
{
    public Guid Id { get; set; }
    public Guid HallId { get; set; }
    public decimal Price { get; set; }
    public List<Guid> Favors { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}
