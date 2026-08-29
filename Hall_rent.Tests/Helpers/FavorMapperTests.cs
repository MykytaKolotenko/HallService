using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Helpers;
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
            new() { Id = id, Name = "Projector", Price = 50m }
        };

        var result = FavorMapper.ToDto(entities);

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
            new() { Id = id, Name = "Parking", Price = 20m }
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
}