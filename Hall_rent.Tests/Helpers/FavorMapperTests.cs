using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Mappers;
using Xunit;

namespace Hall_rent.Tests.Helpers;

public sealed class FavorMapperTests
{
    [Fact]
    public void ToDto_ShouldMapAllProperties()
    {
        var id = Guid.NewGuid();
        var entities = new List<FavorEntity>
        {
            new FavorEntity { Id = id, Name = "Projector", Price = 50m }
        };

        var result = entities.Select(FavorMapper.ToDto).ToList();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(id);
        result[0].Name.Should().Be("Projector");
        result[0].Price.Should().Be(50m);
    }

    [Fact]
    public void ToResponse_ShouldMapAllProperties()
    {
        var id = Guid.NewGuid();
        var entities = new List<FavorEntity>
        {
            new FavorEntity { Id = id, Name = "Parking", Price = 20m }
        };

        var result = FavorMapper.ToResponse(entities);

        result.Should().ContainSingle();
        result[0].Id.Should().Be(id);
        result[0].Name.Should().Be("Parking");
        result[0].Price.Should().Be(20m);
    }

    [Fact]
    public void ToResponse_ShouldReturnEmptyCollection_WhenInputIsEmpty()
    {
        FavorMapper.ToResponse([]).Should().BeEmpty();
    }

    [Fact]
    public void ToEntity_ShouldSnapshotFavorAndBookingPriceAndForeignKeys()
    {
        var favor = new FavorEntity
        {
            Id = Guid.NewGuid(),
            Name = "Projector",
            Price = 50m
        };
        var booking = new HallBookingEntity { Id = Guid.NewGuid() };

        var result = FavorMapper.ToEntity(favor, booking);

        result.HallBookingId.Should().Be(booking.Id);
        result.Booking.Should().BeSameAs(booking);
        result.FavorId.Should().Be(favor.Id);
        result.Favor.Should().BeSameAs(favor);
        result.PriceAtBooking.Should().Be(50m);
    }
}