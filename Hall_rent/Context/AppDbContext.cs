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
    public DbSet<FavorEntity> Favors { get; set; } = null!;

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

        modelBuilder.Entity<HallBookingFavorEntity>()
            .Property(x => x.PriceAtBooking)
            .HasPrecision(18, 2);

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

        modelBuilder.Entity<HallFavorEntity>(builder =>
        {
            builder.ToTable("HallFavorLinks");

            builder.HasKey(x => new
            {
                x.HallId,
                x.FavorId
            });

            builder
                .HasOne(x => x.Hall)
                .WithMany(x => x.Favors)
                .HasForeignKey(x => x.HallId);

            builder
                .HasOne(x => x.Favor)
                .WithMany(x => x.Halls)
                .HasForeignKey(x => x.FavorId);
        });
    }
}
