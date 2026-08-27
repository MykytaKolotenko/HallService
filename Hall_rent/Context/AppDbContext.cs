using Hall_rent.Entity;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<HallEntity> Halls { get; set; } = null!;
    public DbSet<HallBookingEntity> Bookings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HallEntity>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<HallBookingEntity>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Price).HasPrecision(18, 2);
        });
    }
}
