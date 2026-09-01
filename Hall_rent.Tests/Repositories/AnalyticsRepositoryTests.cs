using FluentAssertions;
using Hall_rent.Context;
using Hall_rent.Entity;
using Hall_rent.Repository;
using Hall_rent.Row;
using Hall_rent.Tests.Support;
using Xunit;

namespace Hall_rent.Tests.Repositories;

public sealed class AnalyticsRepositoryTests
{
    [Fact]
    public async Task GetByPeriodAsync_ShouldGroupRevenueByBookingStartDay()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var day1 = Utc(2030, 1, 1, 10);
        var day2 = Utc(2030, 1, 2, 10);

        db.Bookings.AddRange(
            Booking(day1, 100m),
            Booking(day1.AddHours(4), 50m),
            Booking(day2, 200m));
        await db.SaveChangesAsync();

        var result = await Sut(db).GetByPeriodAsync(day1.Date, day2.Date.AddDays(1));

        result.Should().HaveCount(2);
        result[0].Should().BeEquivalentTo(
            new HallRevenueRow(
                day1.Date,
                150m,
                2));

        result[1].Should().BeEquivalentTo(
            new HallRevenueRow(
                day2.Date,
                200m,
                1));
    }

    [Fact]
    public async Task GetByPeriodAsync_ShouldUseHalfOpenRange()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var from = Utc(2030, 1, 1, 10);
        var to = from.AddDays(1);

        db.Bookings.AddRange(
            Booking(from, 100m),
            Booking(to, 999m),
            Booking(from.AddDays(-1), 888m));
        await db.SaveChangesAsync();

        var result = await Sut(db).GetByPeriodAsync(from, to);

        result.Should().ContainSingle();
        result[0].Revenue.Should().Be(100m);
        result[0].BookingsCount.Should().Be(1);
    }

    [Fact]
    public async Task GetByPeriodAsync_ShouldReturnEmptyList_WhenNoBookingsInRange()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = Sut(db);

        var result = await repository.GetByPeriodAsync(Utc(2030, 1, 1), Utc(2030, 1, 31));

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldSumHistoricalPriceAtBooking_NotCurrentFavorPrice()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var favor = Favor("Projector", 60m);
        var firstBooking = Booking(Utc(2030, 1, 1, 10), 150m);
        var secondBooking = Booking(Utc(2030, 1, 2, 10), 150m);

        db.Favors.Add(favor);
        db.Bookings.AddRange(firstBooking, secondBooking);
        db.HallBookingFavors.AddRange(
            BookingFavor(firstBooking, favor, 50m),
            BookingFavor(secondBooking, favor, 50m));
        await db.SaveChangesAsync();

        var result = await Sut(db).GetTopFavorsAsync(
            Utc(2030, 1, 1), Utc(2030, 1, 3), 10);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(favor.Id);
        result[0].BookingsCount.Should().Be(2);
        result[0].Revenue.Should().Be(100m,
            "analytics must use the historical PriceAtBooking, not the favor's current price");
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldNotSplitFavorAcrossDifferentHistoricalPrices()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var favor = Favor("Catering", 70m);
        var firstBooking = Booking(Utc(2030, 1, 1, 10), 150m);
        var secondBooking = Booking(Utc(2030, 1, 2, 10), 160m);

        db.Favors.Add(favor);
        db.Bookings.AddRange(firstBooking, secondBooking);
        db.HallBookingFavors.AddRange(
            BookingFavor(firstBooking, favor, 50m),
            BookingFavor(secondBooking, favor, 60m));
        await db.SaveChangesAsync();

        var result = await Sut(db).GetTopFavorsAsync(
            Utc(2030, 1, 1), Utc(2030, 1, 3), 10);

        result.Should().ContainSingle(
            "one favor must be aggregated by FavorId regardless of historical price changes");
        result[0].BookingsCount.Should().Be(2);
        result[0].Revenue.Should().Be(110m);
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldOrderByRevenueDescendingAndRespectLimit()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var expensive = Favor("Popular", 100m);
        var cheap = Favor("Cheap", 10m);
        var booking = Booking(Utc(2030, 1, 1, 10), 110m);

        db.Favors.AddRange(expensive, cheap);
        db.Bookings.Add(booking);
        db.HallBookingFavors.AddRange(
            BookingFavor(booking, expensive, 100m),
            BookingFavor(booking, cheap, 10m));
        await db.SaveChangesAsync();

        var result = await Sut(db).GetTopFavorsAsync(
            Utc(2030, 1, 1), Utc(2030, 1, 2), 1);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(expensive.Id);
        result[0].Revenue.Should().Be(100m);
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldExcludeFavorsFromBookingsOutsideRange()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var favor = Favor("Music", 30m);
        var inRangeBooking = Booking(Utc(2030, 1, 1, 10), 130m);
        var outOfRangeBooking = Booking(Utc(2030, 3, 1, 10), 130m);

        db.Favors.Add(favor);
        db.Bookings.AddRange(inRangeBooking, outOfRangeBooking);
        db.HallBookingFavors.AddRange(
            BookingFavor(inRangeBooking, favor, 30m),
            BookingFavor(outOfRangeBooking, favor, 30m));
        await db.SaveChangesAsync();

        var result = await Sut(db).GetTopFavorsAsync(
            Utc(2030, 1, 1), Utc(2030, 1, 31), 10);

        result.Should().ContainSingle();
        result[0].BookingsCount.Should().Be(1);
        result[0].Revenue.Should().Be(30m);
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldReturnEmpty_WhenNoBookingsMatchRange()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var favor = Favor("Music", 30m);
        var booking = Booking(Utc(2030, 3, 1, 10), 130m);

        db.Favors.Add(favor);
        db.Bookings.Add(booking);
        db.HallBookingFavors.Add(BookingFavor(booking, favor, 30m));
        await db.SaveChangesAsync();

        var result = await Sut(db).GetTopFavorsAsync(
            Utc(2030, 1, 1), Utc(2030, 1, 31), 10);

        result.Should().BeEmpty();
    }

    private static AnalyticsRepository Sut(AppDbContext db)
    {
        return new AnalyticsRepository(db);
    }

    private static HallBookingEntity Booking(DateTime from, decimal price)
    {
        return new HallBookingEntity
        {
            Id = Guid.NewGuid(),
            HallId = Guid.NewGuid(),
            From = from,
            To = from.AddHours(2),
            Price = price
        };
    }

    private static FavorEntity Favor(string name, decimal price)
    {
        return new FavorEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price
        };
    }

    private static HallBookingFavorEntity BookingFavor(
        HallBookingEntity booking,
        FavorEntity favor,
        decimal priceAtBooking)
    {
        return new HallBookingFavorEntity
        {
            Id = Guid.NewGuid(),
            HallBookingId = booking.Id,
            FavorId = favor.Id,
            PriceAtBooking = priceAtBooking
        };
    }

    private static DateTime Utc(int year, int month, int day, int hour = 0)
    {
        return new DateTime(year, month, day, hour, 0, 0, DateTimeKind.Utc);
    }
}