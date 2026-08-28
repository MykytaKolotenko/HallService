using Hall_rent.Entity;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<HallEntity> Halls { get; set; } = null!;
    public DbSet<HallBookingEntity> Bookings { get; set; } = null!;
    public DbSet<FavorEntity> Favours { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HallEntity>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(255);
            builder.HasIndex(x => x.Name).IsUnique();
            builder.Property(x => x.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<HallBookingEntity>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<FavorEntity>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Price).HasPrecision(18, 2);
        });
    }
}