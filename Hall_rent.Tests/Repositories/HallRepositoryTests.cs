using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Repository;
using Hall_rent.Tests.Support;
using Xunit;

namespace Hall_rent.Tests.Repositories;

public sealed class HallRepositoryTests
{
    [Fact]
    public async Task AddAsync_AndGetByIdAsync_ShouldPersistHall()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new HallRepository(db);
        var hall = new HallEntity
        {
            Id = Guid.NewGuid(),
            Name = "Main Hall",
            Persons = 20,
            Price = 100m,
            Favors = []
        };

        await repository.AddAsync(hall);
        await db.SaveChangesAsync();

        var result = await repository.GetByIdWithFavorsAsync(hall.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Main Hall");
        result.Persons.Should().Be(20);
        result.Price.Should().Be(100m);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenMissing()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new HallRepository(db);

        (await repository.GetByIdWithFavorsAsync(Guid.NewGuid())).Should().BeNull();
    }

    [Fact]
    public async Task Remove_ShouldMarkHallForDeletion()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new HallRepository(db);
        var hall = new HallEntity { Id = Guid.NewGuid(), Name = "Hall", Persons = 10, Price = 50m };
        db.Halls.Add(hall);
        await db.SaveChangesAsync();

        repository.Remove(hall);
        await db.SaveChangesAsync();

        (await repository.GetByIdWithFavorsAsync(hall.Id)).Should().BeNull();
    }

    [Fact]
    public async Task FindAvailableHallsAsync_ShouldFilterByCapacity()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new HallRepository(db);
        var enough = new HallEntity { Id = Guid.NewGuid(), Name = "Enough", Persons = 20, Price = 100m };
        var tooSmall = new HallEntity { Id = Guid.NewGuid(), Name = "Small", Persons = 5, Price = 80m };
        db.Halls.AddRange(enough, tooSmall);
        await db.SaveChangesAsync();
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(2);

        var result = await repository.FindAvailableHallsAsync(start, end, 10);

        result.Select(x => x.Id).Should().Contain(enough.Id).And.NotContain(tooSmall.Id);
    }

    [Fact]
    public async Task FindAvailableHallsAsync_ShouldExcludeOverlappingBooking()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new HallRepository(db);
        var free = new HallEntity { Id = Guid.NewGuid(), Name = "Free", Persons = 20, Price = 100m };
        var booked = new HallEntity { Id = Guid.NewGuid(), Name = "Booked", Persons = 20, Price = 100m };
        db.Halls.AddRange(free, booked);
        var start = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(2);
        db.Bookings.Add(new HallBookingEntity
        {
            Id = Guid.NewGuid(),
            HallId = booked.Id,
            From = start.AddMinutes(30),
            To = end.AddHours(1),
            Price = 100m,
            Favors = []
        });
        await db.SaveChangesAsync();

        var result = await repository.FindAvailableHallsAsync(start, end, 10);

        result.Select(x => x.Id).Should().Equal(free.Id);
    }

    [Fact]
    public async Task FindAvailableHallsAsync_ShouldAllowBookingStartingExactlyWhenPreviousEnds()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new HallRepository(db);
        var hall = new HallEntity { Id = Guid.NewGuid(), Name = "Hall", Persons = 20, Price = 100m };
        db.Halls.Add(hall);
        var previousStart = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var previousEnd = previousStart.AddHours(2);
        db.Bookings.Add(new HallBookingEntity
        {
            Id = Guid.NewGuid(), HallId = hall.Id,
            From = previousStart, To = previousEnd,
            Price = 100m, Favors = []
        });
        await db.SaveChangesAsync();

        var result = await repository.FindAvailableHallsAsync(previousEnd, previousEnd.AddHours(2), 10);

        result.Should().ContainSingle().Which.Id.Should().Be(hall.Id);
    }

    [Fact]
    public async Task FindAvailableHallsAsync_ShouldAllowSearchEndingExactlyWhenExistingStarts()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new HallRepository(db);
        var hall = new HallEntity { Id = Guid.NewGuid(), Name = "Hall", Persons = 20, Price = 100m };
        db.Halls.Add(hall);
        var existingStart = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        db.Bookings.Add(new HallBookingEntity
        {
            Id = Guid.NewGuid(), HallId = hall.Id,
            From = existingStart, To = existingStart.AddHours(2),
            Price = 100m, Favors = []
        });
        await db.SaveChangesAsync();

        var result = await repository.FindAvailableHallsAsync(existingStart.AddHours(-2), existingStart, 10);

        result.Should().ContainSingle().Which.Id.Should().Be(hall.Id);
    }
}