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
    private readonly Mock<IFavorService> _service = new Mock<IFavorService>();

    private FavorController Sut()
    {
        return new FavorController(
            _service.Object,
            new FavorUpdateRequestValidator(),
            new FavorCreateRequestValidator());
    }

    [Fact]
    public async Task GetFavors_ShouldReturnOkWithServiceResult()
    {
        List<FavorResponse> data = new List<FavorResponse>
        {
            new FavorResponse { Id = Guid.NewGuid(), Name = "Wi-Fi", Price = 10m }
        };
        _service.Setup(x => x.GetFavors()).ReturnsAsync(data);

        ActionResult<List<FavorResponse>> result = await Sut().GetFavors();

        OkObjectResult ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
    }

    [Fact]
    public async Task CreateFavors_ShouldReturnCreatedResult()
    {
        Guid id = Guid.NewGuid();
        FavorCreateRequest request = new FavorCreateRequest { Name = "Projector", Price = 50m };
        _service.Setup(x => x.AddFavor(request)).ReturnsAsync(id);

        ActionResult<Guid> result = await Sut().CreateFavors(request);

        CreatedAtActionResult created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(FavorController.CreateFavors));
        created.Value.Should().Be(id);
        _service.Verify(x => x.AddFavor(request), Times.Once);
    }

    [Fact]
    public async Task CreateFavors_ShouldRejectInvalidRequestBeforeService()
    {
        FavorCreateRequest request = new FavorCreateRequest { Name = "", Price = 0m };

        Func<Task<ActionResult<Guid>>> act = () => Sut().CreateFavors(request);

        await act.Should().ThrowAsync<ValidationException>();
        _service.Verify(x => x.AddFavor(It.IsAny<FavorCreateRequest>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFavors_ShouldPassRouteIdToService()
    {
        Guid id = Guid.NewGuid();
        FavorUpdateRequest request = new FavorUpdateRequest { Name = "New", Price = 25m };

        IActionResult result = await Sut().UpdateFavors(id, request);

        result.Should().BeOfType<OkResult>();
        _service.Verify(x => x.UpdateFavor(It.Is<UpdateFavorDto>(d =>
            d.Id == id && d.Name == "New" && d.Price == 25m)), Times.Once);
    }

    [Fact]
    public async Task DeleteFavors_ShouldPassIdToService()
    {
        Guid id = Guid.NewGuid();

        IActionResult result = await Sut().DeleteFavors(id);

        result.Should().BeOfType<OkResult>();
        _service.Verify(x => x.DeleteFavor(id), Times.Once);
    }
}