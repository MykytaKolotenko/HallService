using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Repository;
using Hall_rent.Tests.Support;
using Xunit;

namespace Hall_rent.Tests.Repositories;

public sealed class AnalyticsRepositoryTests
{
    [Fact]
    public async Task GetByPeriodAsync_ShouldGroupRevenueByDay()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var day1 = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var day2 = new DateTime(2030, 1, 2, 10, 0, 0, DateTimeKind.Utc);

        db.Bookings.AddRange(
            new HallBookingEntity { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = day1, To = day1.AddHours(2), Price = 100m },
            new HallBookingEntity { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = day1.AddHours(4), To = day1.AddHours(6), Price = 50m },
            new HallBookingEntity { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = day2, To = day2.AddHours(2), Price = 200m });
        await db.SaveChangesAsync();

        var repository = new AnalyticsRepository(db);

        var result = await repository.GetByPeriodAsync(day1.Date, day2.Date.AddDays(1));

        result.Should().HaveCount(2);
        result[0].Day.Should().Be(day1.Date);
        result[0].Revenue.Should().Be(150m);
        result[0].BookingsCount.Should().Be(2);
        result[1].Day.Should().Be(day2.Date);
        result[1].Revenue.Should().Be(200m);
        result[1].BookingsCount.Should().Be(1);
    }

    [Fact]
    public async Task GetByPeriodAsync_ShouldExcludeBookingsOutsideRange()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var inRange = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var outOfRange = new DateTime(2030, 2, 1, 10, 0, 0, DateTimeKind.Utc);

        db.Bookings.AddRange(
            new HallBookingEntity { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = inRange, To = inRange.AddHours(2), Price = 100m },
            new HallBookingEntity { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = outOfRange, To = outOfRange.AddHours(2), Price = 999m });
        await db.SaveChangesAsync();

        var repository = new AnalyticsRepository(db);

        var result = await repository.GetByPeriodAsync(new DateTime(2030, 1, 1), new DateTime(2030, 1, 31));

        result.Should().ContainSingle();
        result[0].Revenue.Should().Be(100m);
    }

    [Fact]
    public async Task GetByPeriodAsync_ShouldReturnEmptyList_WhenNoBookingsInRange()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new AnalyticsRepository(db);

        var result = await repository.GetByPeriodAsync(new DateTime(2030, 1, 1), new DateTime(2030, 1, 31));

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldSumHistoricalPriceAtBooking_NotCurrentFavorPrice()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var favorId = Guid.NewGuid();
        var from = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        db.Favors.Add(new FavorEntity { Id = favorId, Name = "Projector", Price = 60m });

        var booking1 = new HallBookingEntity { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = from, To = from.AddHours(2), Price = 150m };
        var booking2 = new HallBookingEntity
            { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = from.AddDays(1), To = from.AddDays(1).AddHours(2), Price = 150m };
        db.Bookings.AddRange(booking1, booking2);
        db.Set<HallBookingFavorEntity>().AddRange(
            new HallBookingFavorEntity { Id = Guid.NewGuid(), HallBookingId = booking1.Id, FavorId = favorId, PriceAtBooking = 50m },
            new HallBookingFavorEntity { Id = Guid.NewGuid(), HallBookingId = booking2.Id, FavorId = favorId, PriceAtBooking = 50m });
        await db.SaveChangesAsync();

        var repository = new AnalyticsRepository(db);

        var result = await repository.GetTopFavorsAsync(from, from.AddDays(2), 10);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(favorId);
        result[0].BookingsCount.Should().Be(2);
        result[0].Revenue.Should().Be(100m, "выручка должна считаться по PriceAtBooking (50+50), а не по текущей цене (60*2=120)");
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldNotSplitFavorAcrossDifferentHistoricalPrices()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var favorId = Guid.NewGuid();
        var from = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        db.Favors.Add(new FavorEntity { Id = favorId, Name = "Catering", Price = 70m });

        var booking1 = new HallBookingEntity { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = from, To = from.AddHours(2), Price = 150m };
        var booking2 = new HallBookingEntity
            { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = from.AddDays(1), To = from.AddDays(1).AddHours(2), Price = 160m };
        db.Bookings.AddRange(booking1, booking2);
        db.Set<HallBookingFavorEntity>().AddRange(
            new HallBookingFavorEntity { Id = Guid.NewGuid(), HallBookingId = booking1.Id, FavorId = favorId, PriceAtBooking = 50m },
            new HallBookingFavorEntity { Id = Guid.NewGuid(), HallBookingId = booking2.Id, FavorId = favorId, PriceAtBooking = 60m });
        await db.SaveChangesAsync();

        var repository = new AnalyticsRepository(db);

        var result = await repository.GetTopFavorsAsync(from, from.AddDays(2), 10);

        result.Should().ContainSingle("услуга не должна распадаться на несколько строк из-за разных исторических цен");
        result[0].BookingsCount.Should().Be(2);
        result[0].Revenue.Should().Be(110m);
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldOrderByRevenueDescendingAndRespectLimit()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var popularFavor = Guid.NewGuid();
        var cheapFavor = Guid.NewGuid();
        var from = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);

        db.Favors.AddRange(
            new FavorEntity { Id = popularFavor, Name = "Popular", Price = 100m },
            new FavorEntity { Id = cheapFavor, Name = "Cheap", Price = 10m });

        var booking = new HallBookingEntity { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = from, To = from.AddHours(2), Price = 110m };
        db.Bookings.Add(booking);
        db.Set<HallBookingFavorEntity>().AddRange(
            new HallBookingFavorEntity { Id = Guid.NewGuid(), HallBookingId = booking.Id, FavorId = popularFavor, PriceAtBooking = 100m },
            new HallBookingFavorEntity { Id = Guid.NewGuid(), HallBookingId = booking.Id, FavorId = cheapFavor, PriceAtBooking = 10m });
        await db.SaveChangesAsync();

        var repository = new AnalyticsRepository(db);

        var result = await repository.GetTopFavorsAsync(from, from.AddDays(1), 1);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(popularFavor);
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldExcludeFavorsFromBookingsOutsideRange()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var favorId = Guid.NewGuid();
        var inRange = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var outOfRange = new DateTime(2030, 3, 1, 10, 0, 0, DateTimeKind.Utc);

        db.Favors.Add(new FavorEntity { Id = favorId, Name = "Music", Price = 30m });

        var bookingInRange = new HallBookingEntity { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = inRange, To = inRange.AddHours(2), Price = 130m };
        var bookingOutOfRange = new HallBookingEntity
            { Id = Guid.NewGuid(), HallId = Guid.NewGuid(), From = outOfRange, To = outOfRange.AddHours(2), Price = 130m };
        db.Bookings.AddRange(bookingInRange, bookingOutOfRange);
        db.Set<HallBookingFavorEntity>().AddRange(
            new HallBookingFavorEntity { Id = Guid.NewGuid(), HallBookingId = bookingInRange.Id, FavorId = favorId, PriceAtBooking = 30m },
            new HallBookingFavorEntity { Id = Guid.NewGuid(), HallBookingId = bookingOutOfRange.Id, FavorId = favorId, PriceAtBooking = 30m });
        await db.SaveChangesAsync();

        var repository = new AnalyticsRepository(db);

        var result = await repository.GetTopFavorsAsync(new DateTime(2030, 1, 1), new DateTime(2030, 1, 31), 10);

        result.Should().ContainSingle();
        result[0].BookingsCount.Should().Be(1);
        result[0].Revenue.Should().Be(30m);
    }
}
