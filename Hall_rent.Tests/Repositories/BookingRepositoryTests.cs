using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Repository;
using Hall_rent.Tests.Support;
using Xunit;

namespace Hall_rent.Tests.Repositories;

public sealed class BookingRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldPersistBooking()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new BookingRepository(db);
        var hallId = Guid.NewGuid();
        var booking = new HallBookingEntity
        {
            Id = Guid.NewGuid(), HallId = hallId, Price = 100m,
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Favors = []
        };

        await repository.AddAsync(booking);
        await db.SaveChangesAsync();

        db.Bookings.Should().ContainSingle(x => x.Id == booking.Id);
    }

    [Fact]
    public async Task IsHallAvailableAsync_ShouldReturnTrue_WhenThereIsNoOverlap()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new BookingRepository(db);
        var hallId = Guid.NewGuid();
        var start = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(2);
        db.Bookings.Add(new HallBookingEntity
        {
            Id = Guid.NewGuid(), HallId = hallId, Price = 100m,
            StartAt = start, EndAt = end, Favors = []
        });
        await db.SaveChangesAsync();

        (await repository.IsHallAvailableAsync(hallId, end, end.AddHours(2))).Should().BeTrue();
    }

    [Fact]
    public async Task IsHallAvailableAsync_ShouldReturnFalse_WhenIntervalsOverlap()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new BookingRepository(db);
        var hallId = Guid.NewGuid();
        var start = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        db.Bookings.Add(new HallBookingEntity
        {
            Id = Guid.NewGuid(), HallId = hallId, Price = 100m,
            StartAt = start, EndAt = start.AddHours(2), Favors = []
        });
        await db.SaveChangesAsync();

        (await repository.IsHallAvailableAsync(hallId, start.AddMinutes(1), start.AddHours(3))).Should().BeFalse();
    }

    [Fact]
    public async Task IsHallAvailableAsync_ShouldIgnoreBookingsOfAnotherHall()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new BookingRepository(db);
        var hallId = Guid.NewGuid();
        var otherHallId = Guid.NewGuid();
        var start = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        db.Bookings.Add(new HallBookingEntity
        {
            Id = Guid.NewGuid(), HallId = otherHallId, Price = 100m,
            StartAt = start, EndAt = start.AddHours(2), Favors = []
        });
        await db.SaveChangesAsync();

        (await repository.IsHallAvailableAsync(hallId, start.AddMinutes(30), start.AddHours(1))).Should().BeTrue();
    }
}