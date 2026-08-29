using FluentAssertions;
using FluentValidation;
using Hall_rent.Controllers;
using Hall_rent.Dto;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service;
using Hall_rent.Validation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Controllers;

public sealed class HallControllerTests
{
    private readonly Mock<IBookingService> _bookingService = new();
    private readonly Mock<IHallService> _hallService = new();

    private HallController Sut() => new(
        _hallService.Object,
        _bookingService.Object,
        new HallCreateRequestValidator(),
        new HallSearchRequestValidator(new FixedClock(DateTime.UtcNow)),
        new HallUpdateRequestValidator(),
        new HallBookRequestValidator());

    [Fact]
    public async Task CreateHall_ShouldCallServiceAndReturnOk()
    {
        var id = Guid.NewGuid();
        _hallService.Setup(x => x.AddHall(It.IsAny<HallCreateDto>())).ReturnsAsync(id);
        var request = new HallCreateRequest { Name = "Hall", Persons = 20, Price = 100m, Favors = [] };

        var result = await Sut().CreateHall(request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
        _hallService.Verify(x => x.AddHall(It.Is<HallCreateDto>(d =>
            d.Name == "Hall" && d.Persons == 20 && d.Price == 100m)), Times.Once);
    }

    [Fact]
    public async Task CreateHall_ShouldRejectInvalidRequestBeforeService()
    {
        var request = new HallCreateRequest { Name = "", Persons = 0, Price = 0m };

        var act = () => Sut().CreateHall(request);

        await act.Should().ThrowAsync<ValidationException>();
        _hallService.Verify(x => x.AddHall(It.IsAny<HallCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task PatchHall_ShouldPassRouteIdToService()
    {
        var id = Guid.NewGuid();
        var request = new HallUpdateRequest { Persons = 10, Price = 200m, Favors = [] };

        var result = await Sut().PatchHall(id, request);

        result.Result.Should().BeOfType<OkResult>();
        _hallService.Verify(x => x.UpdateHall(It.Is<UpdateHallDto>(d =>
            d.Id == id && d.Persons == 10 && d.Price == 200m && d.Favors!.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task PatchHall_ShouldRejectNullFavorsBeforeService()
    {
        var id = Guid.NewGuid();
        var request = new HallUpdateRequest { Persons = 10, Price = 200m, Favors = null };

        var act = () => Sut().PatchHall(id, request);

        await act.Should().ThrowAsync<ValidationException>();
        _hallService.Verify(x => x.UpdateHall(It.IsAny<UpdateHallDto>()), Times.Never);
    }

    [Fact]
    public async Task DeleteHall_ShouldCallService()
    {
        var id = Guid.NewGuid();
        var result = await Sut().DeleteHall(id);

        result.Should().BeOfType<OkResult>();
        _hallService.Verify(x => x.DeleteHall(id), Times.Once);
    }

    [Fact]
    public async Task BookHall_ShouldPassRouteAndBodyAndReturnOk()
    {
        var hallId = Guid.NewGuid();
        var favorId = Guid.NewGuid();
        var request = new HallBookRequest
        {
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Persons = 10,
            Favors = [favorId]
        };
        var response = new HallBookResponse { Id = Guid.NewGuid(), Price = 150m };
        _bookingService.Setup(x => x.BookAsync(It.IsAny<BookHallDto>())).ReturnsAsync(response);

        var result = await Sut().BookHall(hallId, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(response);
        _bookingService.Verify(x => x.BookAsync(It.Is<BookHallDto>(d =>
            d.HallId == hallId &&
            d.Persons == 10 &&
            d.Favors.SequenceEqual(new List<Guid> { favorId }) &&
            d.StartAt == request.StartAt &&
            d.EndAt == request.EndAt)), Times.Once);
    }

    [Fact]
    public async Task BookHall_ShouldRejectInvalidBodyBeforeService()
    {
        var request = new HallBookRequest
        {
            StartAt = DateTime.UtcNow.AddMinutes(-5),
            EndAt = DateTime.UtcNow.AddMinutes(-1),
            Persons = 0,
            Favors = []
        };

        var act = () => Sut().BookHall(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<ValidationException>();
        _bookingService.Verify(x => x.BookAsync(It.IsAny<BookHallDto>()), Times.Never);
    }

    [Fact]
    public async Task SearchHalls_ShouldPassQueryToService()
    {
        var request = new HallSearchRequest
        {
            StartAt = DateTime.UtcNow.AddHours(2),
            EndAt = DateTime.UtcNow.AddHours(3),
            Persons = 10
        };
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _hallService.Setup(x => x.FindAvailableHallIdsAsync(It.IsAny<HallSearchDto>())).ReturnsAsync(ids);

        var result = await Sut().SearchHalls(request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(ids);
        _hallService.Verify(x => x.FindAvailableHallIdsAsync(It.Is<HallSearchDto>(d =>
            d.StartAt == request.StartAt && d.EndAt == request.EndAt && d.Persons == request.Persons)), Times.Once);
    }
}