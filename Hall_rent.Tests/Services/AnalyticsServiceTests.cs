using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Row;
using Hall_rent.Service;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services;

public sealed class AnalyticsServiceTests
{
    private readonly Mock<IAnalyticsRepository> _repository = new Mock<IAnalyticsRepository>();

    private AnalyticsService Sut()
    {
        return new AnalyticsService(_repository.Object);
    }

    [Fact]
    public async Task GetRevenueReportAsync_ShouldSumTotalsAndMapRows()
    {
        var day1 = new DateTime(2030, 1, 1);
        var day2 = new DateTime(2030, 1, 2);
        var request = new DateRangeDto { From = day1, To = day2.AddDays(1) };

        _repository
            .Setup(x => x.GetByPeriodAsync(request.From, request.To))
            .ReturnsAsync([
                new HallRevenueRow(day1, 150m, 2),
                new HallRevenueRow(day2, 200m, 1)
            ]);

        var result = await Sut().GetRevenueReportAsync(request);

        result.TotalRevenue.Should().Be(350m);
        result.TotalBookings.Should().Be(3);
        result.RevenuePerDay.Should().HaveCount(2);
        result.RevenuePerDay[0].Day.Should().Be(day1);
        result.RevenuePerDay[0].Revenue.Should().Be(150m);
        result.RevenuePerDay[0].BookingsCount.Should().Be(2);
        result.RevenuePerDay[1].Day.Should().Be(day2);
    }

    [Fact]
    public async Task GetRevenueReportAsync_ShouldReturnZeroTotals_WhenNoData()
    {
        var request = new DateRangeDto { From = DateTime.UtcNow, To = DateTime.UtcNow.AddDays(1) };

        _repository
            .Setup(x => x.GetByPeriodAsync(request.From, request.To))
            .ReturnsAsync([]);

        var result = await Sut().GetRevenueReportAsync(request);

        result.TotalRevenue.Should().Be(0m);
        result.TotalBookings.Should().Be(0);
        result.RevenuePerDay.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldSumTotalsAndMapRows()
    {
        var favorId1 = Guid.NewGuid();
        var favorId2 = Guid.NewGuid();
        var request = new DateRangeDto { From = DateTime.UtcNow, To = DateTime.UtcNow.AddDays(1) };

        _repository
            .Setup(x => x.GetTopFavorsAsync(request.From, request.To, 5))
            .ReturnsAsync([
                new FavorRevenueRow { Id = favorId1, Name = "Projector", BookingsCount = 3, Revenue = 150m },
                new FavorRevenueRow { Id = favorId2, Name = "Catering", BookingsCount = 2, Revenue = 100m }
            ]);

        var result = await Sut().GetTopFavorsAsync(request, 5);

        result.Revenue.Should().Be(250m);
        result.BookingsCount.Should().Be(5);
        result.Favors.Should().HaveCount(2);
        result.Favors[0].Id.Should().Be(favorId1);
        result.Favors[0].Name.Should().Be("Projector");
        result.Favors[0].Revenue.Should().Be(150m);
        result.Favors[0].BookingsCount.Should().Be(3);

        _repository.Verify(x => x.GetTopFavorsAsync(request.From, request.To, 5), Times.Once);
    }

    [Fact]
    public async Task GetTopFavorsAsync_ShouldPassDefaultLimit_WhenNotSpecified()
    {
        var request = new DateRangeDto { From = DateTime.UtcNow, To = DateTime.UtcNow.AddDays(1) };

        _repository
            .Setup(x => x.GetTopFavorsAsync(request.From, request.To, 10))
            .ReturnsAsync([]);

        await Sut().GetTopFavorsAsync(request);

        _repository.Verify(x => x.GetTopFavorsAsync(request.From, request.To, 10), Times.Once);
    }
}