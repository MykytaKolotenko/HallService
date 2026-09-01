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

public sealed class HallControllerTests
{
    private readonly Mock<IBookingService> _bookingService = new Mock<IBookingService>();
    private readonly Mock<IHallService> _hallService = new Mock<IHallService>();

    private HallController Sut()
    {
        return new HallController(
            _hallService.Object,
            new HallCreateRequestValidator(),
            new HallSearchRequestValidator(new FixedClock(DateTime.UtcNow)),
            new HallUpdateRequestValidator());
    }

    [Fact]
    public async Task CreateHall_ShouldCallServiceAndReturnOk()
    {
        var id = Guid.NewGuid();
        _hallService.Setup(x => x.CreateHall(It.IsAny<HallCreateDto>())).ReturnsAsync(new HallCreateResponse(id));
        var request = new HallCreateRequest { Name = "Hall", Persons = 20, Price = 100m, Favors = [] };

        var result = await Sut().CreateHall(request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().NotBeNull();
        _hallService.Verify(x => x.CreateHall(It.Is<HallCreateDto>(d =>
            d.Name == "Hall" && d.Persons == 20 && d.Price == 100m)), Times.Once);
    }

    [Fact]
    public async Task CreateHall_ShouldRejectInvalidRequestBeforeService()
    {
        var request = new HallCreateRequest { Name = "", Persons = 0, Price = 0m };

        var act = () => Sut().CreateHall(request);

        await act.Should().ThrowAsync<ValidationException>();
        _hallService.Verify(x => x.CreateHall(It.IsAny<HallCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task PatchHall_ShouldPassRouteIdToService()
    {
        var id = Guid.NewGuid();
        var request = new HallUpdateRequest { Name = "Hall", Persons = 10, Price = 200m, Favors = [] };

        var result = await Sut().PatchHall(id, request);

        result.Result.Should().BeOfType<OkObjectResult>();
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
    public async Task SearchHalls_ShouldPassQueryToService()
    {
        var request = new HallSearchRequest
        {
            From = DateTime.UtcNow.AddHours(2),
            To = DateTime.UtcNow.AddHours(3),
            Persons = 10
        };
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _hallService.Setup(x => x.SearchAvailableHallIdsAsync(It.IsAny<HallSearchDto>())).ReturnsAsync(new HallSearchResponse(ids));

        var result = await Sut().SearchHalls(request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeEquivalentTo(new HallSearchResponse(ids));
        _hallService.Verify(x => x.SearchAvailableHallIdsAsync(It.Is<HallSearchDto>(d =>
            d.From == request.From && d.To == request.To && d.Persons == request.Persons)), Times.Once);
    }
}