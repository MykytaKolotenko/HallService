using FluentAssertions;
using FluentValidation;
using Hall_rent.Controllers;
using Hall_rent.Dto;
using Hall_rent.Mappers;
using Hall_rent.Request;
using Hall_rent.Response;
using Hall_rent.Service.Interface;
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
        var data = new List<FavorResponse>
        {
            new FavorResponse { Id = Guid.NewGuid(), Name = "Wi-Fi", Price = 10m }
        };
        _service.Setup(x => x.GetFavors()).ReturnsAsync(data);

        var result = await Sut().GetFavors();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeSameAs(data);
    }

    [Fact]
    public async Task CreateFavors_ShouldReturnCreatedResult()
    {
        var id = Guid.NewGuid();
        var request = new FavorCreateRequest { Name = "Projector", Price = 50m };
        var dto = FavorMapper.ToDto(request);
        _service.Setup(x => x.AddFavor(dto)).ReturnsAsync(new FavorCreateResponse(id));

        var result = await Sut().CreateFavors(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(FavorController.CreateFavors));
        created.Value.Should().Be(new FavorCreateResponse(id));
        _service.Verify(x => x.AddFavor(dto), Times.Once);
    }

    [Fact]
    public async Task CreateFavors_ShouldRejectInvalidRequestBeforeService()
    {
        var request = new FavorCreateRequest { Name = "", Price = 0m };

        var act = () => Sut().CreateFavors(request);

        await act.Should().ThrowAsync<ValidationException>();
        _service.Verify(x => x.AddFavor(It.IsAny<FavorCreateDto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFavors_ShouldPassRouteIdToService()
    {
        var id = Guid.NewGuid();
        var request = new FavorUpdateRequest { Name = "New", Price = 25m };

        var result = await Sut().UpdateFavors(id, request);

        result.Should().BeOfType<OkResult>();
        _service.Verify(x => x.UpdateFavor(It.Is<UpdateFavorDto>(d =>
            d.Id == id && d.Name == "New" && d.Price == 25m)), Times.Once);
    }
}
