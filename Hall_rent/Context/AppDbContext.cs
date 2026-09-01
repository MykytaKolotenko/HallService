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
    public DbSet<HallBookingFavorEntity> HallBookingFavors { get; set; } = null!;

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

        // HallBookingFavorEntity is a many-to-many relationship between a booking and a service,
// with an additional PriceAtBooking field (a snapshot of the service price at the time of booking;
// see FavorMapper.ToEntity).
        modelBuilder.Entity<HallBookingFavorEntity>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PriceAtBooking).HasPrecision(18, 2);
// Cascade: if a booking is deleted, its service rows (with the price snapshot) are no longer needed
// and are deleted together with it.
            builder
                .HasOne(x => x.Booking)
                .WithMany(x => x.Favors)
                .HasForeignKey(x => x.HallBookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict (not Cascade!): a service from the Favor catalog cannot be deleted if at least one
// historical booking references it — otherwise deleting FavorEntity would silently erase part of
// the booking history and corrupt analytics (see AnalyticsRepository.GetTopFavorsAsync)..
            builder
                .HasOne(x => x.Favor)
                .WithMany()
                .HasForeignKey(x => x.FavorId)
                .OnDelete(DeleteBehavior.Restrict);

            // The same favor cannot be added to the same booking twice.
            builder
                .HasIndex(x => new { x.HallBookingId, x.FavorId })
                .IsUnique();
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