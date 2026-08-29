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

public sealed class FavorControllerTests
{
    private readonly Mock<IFavorService> _service = new();

    private FavorController Sut() => new(
        _service.Object,
        new FavorUpdateRequestValidator(),
        new FavorCreateRequestValidator());

    [Fact]
    public async Task GetFavours_ShouldReturnOkWithServiceResult()
    {
        var data = new List<FavorResponse>
        {
            new() { Id = Guid.NewGuid(), Name = "Wi-Fi", Price = 10m }
        };
        _service.Setup(x => x.GetFavours()).ReturnsAsync(data);

        var result = await Sut().GetFavours();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
    }

    [Fact]
    public async Task CreateFavours_ShouldReturnCreatedResult()
    {
        var id = Guid.NewGuid();
        var request = new FavorCreateRequest { Name = "Projector", Price = 50m };
        _service.Setup(x => x.AddFavour(request)).ReturnsAsync(id);

        var result = await Sut().CreateFavours(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(FavorController.CreateFavours));
        created.Value.Should().Be(id);
        _service.Verify(x => x.AddFavour(request), Times.Once);
    }

    [Fact]
    public async Task CreateFavours_ShouldRejectInvalidRequestBeforeService()
    {
        var request = new FavorCreateRequest { Name = "", Price = 0m };

        var act = () => Sut().CreateFavours(request);

        await act.Should().ThrowAsync<ValidationException>();
        _service.Verify(x => x.AddFavour(It.IsAny<FavorCreateRequest>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFavours_ShouldPassRouteIdToService()
    {
        var id = Guid.NewGuid();
        var request = new FavorUpdateRequest { Name = "New", Price = 25m };

        var result = await Sut().UpdateFavours(id, request);

        result.Should().BeOfType<OkResult>();
        _service.Verify(x => x.UpdateFavour(It.Is<UpdateFavorDto>(d =>
            d.Id == id && d.Name == "New" && d.Price == 25m)), Times.Once);
    }

    [Fact]
    public async Task DeleteFavours_ShouldPassIdToService()
    {
        var id = Guid.NewGuid();

        var result = await Sut().DeleteFavours(id);

        result.Should().BeOfType<OkResult>();
        _service.Verify(x => x.DeleteFavour(id), Times.Once);
    }
}