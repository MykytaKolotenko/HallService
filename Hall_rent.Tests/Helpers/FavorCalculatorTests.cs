using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Helpers;
using Xunit;

namespace Hall_rent.Tests.Helpers;

public sealed class FavorCalculatorTests
{
    [Fact]
    public void Calculate_ShouldReturnStartPrice_WhenThereAreNoFavours()
    {
        FavorCalculator.Calculate(100m, []).Should().Be(100m);
    }

    [Fact]
    public void Calculate_ShouldAddAllFavourPrices()
    {
        var favours = new List<FavorDto>
        {
            new() { Id = Guid.NewGuid(), Name = "A", Price = 10m },
            new() { Id = Guid.NewGuid(), Name = "B", Price = 20.50m },
            new() { Id = Guid.NewGuid(), Name = "C", Price = 4.50m }
        };

        FavorCalculator.Calculate(100m, favours).Should().Be(135m);
    }

    [Fact]
    public void Calculate_ShouldPreserveDecimalPrecision()
    {
        var favours = new List<FavorDto>
        {
            new() { Price = 0.01m },
            new() { Price = 0.02m }
        };

        FavorCalculator.Calculate(0.10m, favours).Should().Be(0.13m);
    }
}