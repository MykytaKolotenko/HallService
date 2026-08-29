using FluentAssertions;
using Hall_rent.Entity;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services.Favor;

public sealed class GetFavoursTests : FavorServiceTestBase
{
    [Fact]
    public async Task GetFavours_ShouldReturnMappedFavours()
    {
        var favours = new List<FavorEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Wi-Fi",
                Price = 10m
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Parking",
                Price = 20m
            }
        };

        FavorRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(favours);

        var sut = CreateSut();

        var result = await sut.GetFavours();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Wi-Fi");
        result[0].Price.Should().Be(10m);
        result[1].Name.Should().Be("Parking");
        result[1].Price.Should().Be(20m);
    }
}
