namespace Hall_rent.Entity;

public class HallBookingFavorEntity
{
    public Guid Id { get; set; }
    public Guid HallBookingId { get; set; }
    public HallBookingEntity Booking { get; set; } = null!;

    public Guid FavorId { get; set; }
    public FavorEntity Favor { get; set; } = null!;

    public decimal PriceAtBooking { get; set; }
}
