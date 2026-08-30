using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Helpers;
using Xunit;

namespace Hall_rent.Tests.Helpers;

public sealed class FavorCalculatorTests
{
    [Fact]
    public void Calculate_ShouldReturnStartPrice_WhenThereAreNoFavors()
    {
        FavorCalculator.Calculate(100m, []).Should().Be(100m);
    }

    [Fact]
    public void Calculate_ShouldAddAllFavorPrices()
    {
        List<FavorDto> favors = new List<FavorDto>
        {
            new FavorDto { Id = Guid.NewGuid(), Name = "A", Price = 10m },
            new FavorDto { Id = Guid.NewGuid(), Name = "B", Price = 20.50m },
            new FavorDto { Id = Guid.NewGuid(), Name = "C", Price = 4.50m }
        };

        FavorCalculator.Calculate(100m, favors).Should().Be(135m);
    }

    [Fact]
    public void Calculate_ShouldPreserveDecimalPrecision()
    {
        List<FavorDto> favors = new List<FavorDto>
        {
            new FavorDto { Price = 0.01m },
            new FavorDto { Price = 0.02m }
        };

        FavorCalculator.Calculate(0.10m, favors).Should().Be(0.13m);
    }
}
