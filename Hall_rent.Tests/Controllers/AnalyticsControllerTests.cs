using FluentAssertions;
using FluentValidation;
using Hall_rent.Controllers;
using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service.Interface;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Controllers;

public sealed class AnalyticsControllerTests
{
    private readonly Mock<IAnalyticsService> _analyticsService = new Mock<IAnalyticsService>();
    private readonly FixedClock _clock = new FixedClock(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    private AnalyticsController Sut()
    {
        return new AnalyticsController(_analyticsService.Object, new DateRangeValidator(), new AnalyticsTopFavorValidator());
    }

    [Fact]
    public async Task GetRevenue_ShouldPassRangeToServiceAndReturnOk()
    {
        var request = new DateRangeRequest
        {
            From = _clock.UtcNow.AddDays(1),
            To = _clock.UtcNow.AddDays(2)
        };
        var response = new RevenueReportResponse { TotalRevenue = 500m, TotalBookings = 3, RevenuePerDay = [] };

        _analyticsService
            .Setup(x => x.GetRevenueReportAsync(It.Is<DateRangeDto>(d => d.From == request.From && d.To == request.To)))
            .ReturnsAsync(response);

        var result = await Sut().GetRevenue(request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetRevenue_ShouldRejectInvalidRange_ToBeforeFrom()
    {
        var request = new DateRangeRequest
        {
            From = _clock.UtcNow.AddDays(2),
            To = _clock.UtcNow.AddDays(1)
        };

        var act = () => Sut().GetRevenue(request);

        await act.Should().ThrowAsync<ValidationException>();
        _analyticsService.Verify(x => x.GetRevenueReportAsync(It.IsAny<DateRangeDto>()), Times.Never);
    }

    [Fact]
    public async Task GetRevenue_ShouldAllowHistoricalDateRange()
    {
        var request = new DateRangeRequest
        {
            From = _clock.UtcNow.AddDays(-30),
            To = _clock.UtcNow.AddDays(-1)
        };
        var response = new RevenueReportResponse { TotalRevenue = 500m, TotalBookings = 3, RevenuePerDay = [] };

        _analyticsService
            .Setup(x => x.GetRevenueReportAsync(It.IsAny<DateRangeDto>()))
            .ReturnsAsync(response);

        var result = await Sut().GetRevenue(request);

        result.Result.Should().BeOfType<OkObjectResult>();
        _analyticsService.Verify(x => x.GetRevenueReportAsync(
            It.Is<DateRangeDto>(d => d.From == request.From && d.To == request.To)), Times.Once);
    }

    [Fact]
    public async Task GetTopFavors_ShouldPassRangeAndLimitToService()
    {
        var request = new AnalyticsTopFavorRequest
        {
            From = _clock.UtcNow.AddDays(1),
            To = _clock.UtcNow.AddDays(2),
            Limit = 5
        };
        var response = new FavorReportResponse { Revenue = 100m, BookingsCount = 2, Favors = [] };

        _analyticsService
            .Setup(x => x.GetTopFavorsAsync(
                It.Is<DateRangeDto>(d => d.From == request.From && d.To == request.To),
                5))
            .ReturnsAsync(response);

        var result = await Sut().GetTopFavors(request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(response);
        _analyticsService.Verify(x => x.GetTopFavorsAsync(It.IsAny<DateRangeDto>(), 5), Times.Once);
    }

    [Fact]
    public async Task GetTopFavors_ShouldUseDefaultLimit_WhenNotProvided()
    {
        var request = new AnalyticsTopFavorRequest
        {
            From = _clock.UtcNow.AddDays(1),
            To = _clock.UtcNow.AddDays(2),
            Limit = 10
        };

        _analyticsService
            .Setup(x => x.GetTopFavorsAsync(It.IsAny<DateRangeDto>(), 10))
            .ReturnsAsync(new FavorReportResponse { Revenue = 0m, BookingsCount = 0, Favors = [] });

        await Sut().GetTopFavors(request);

        _analyticsService.Verify(x => x.GetTopFavorsAsync(It.IsAny<DateRangeDto>(), 10), Times.Once);
    }

    [Fact]
    public async Task GetTopFavors_ShouldRejectMissingDates()
    {
        var request = new AnalyticsTopFavorRequest();

        var act = () => Sut().GetTopFavors(request);

        await act.Should().ThrowAsync<ValidationException>();
        _analyticsService.Verify(x => x.GetTopFavorsAsync(It.IsAny<DateRangeDto>(), It.IsAny<int>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task LimitValidator_ShouldRejectNonPositiveLimit(int limit)
    {
        var validator = new LimitValidator();

        var result = await validator.ValidateAsync(limit);

        result.IsValid.Should().BeFalse();

        result.Errors.Should().ContainSingle();
    }
}
