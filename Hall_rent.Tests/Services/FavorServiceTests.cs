using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Mappers;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Request;
using Hall_rent.Service;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services;

public sealed class FavorServiceTests
{
    private readonly Mock<IFavorRepository> _repository = new Mock<IFavorRepository>();
    private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();

    private FavorService Sut()
    {
        return new FavorService(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task GetFavors_ShouldReturnMappedResults()
    {
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([
            new FavorEntity { Id = id, Name = "Wi-Fi", Price = 10m }
        ]);

        var result = await Sut().GetFavors();

        result.Should().ContainSingle();
        result[0].Id.Should().Be(id);
        result[0].Name.Should().Be("Wi-Fi");
        result[0].Price.Should().Be(10m);
    }

    [Fact]
    public async Task GetFavors_ShouldReturnEmpty_WhenRepositoryReturnsEmpty()
    {
        _repository.Setup(x => x.GetAllAsync()).ReturnsAsync([]);

        (await Sut().GetFavors()).Should().BeEmpty();
    }

    [Fact]
    public async Task AddFavor_ShouldMapAddAndSave()
    {
        var request = new FavorCreateRequest
        {
            Name = "Projector",
            Price = 50m
        };
        var dto = FavorMapper.ToDto(request);


        _repository
            .Setup(x => x.AddAsync(It.IsAny<FavorEntity>()))
            .Callback<FavorEntity>(favor => { favor.Id = Guid.NewGuid(); })
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default(CancellationToken)))
            .Returns(Task.CompletedTask);

        var result = await Sut().AddFavor(dto);

        result.Should().NotBe(Guid.Empty);

        _repository.Verify(
            x => x.AddAsync(It.Is<FavorEntity>(favor =>
                favor.Id != Guid.Empty &&
                favor.Name == request.Name &&
                favor.Price == request.Price)),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Once);
    }

    [Fact]
    public async Task UpdateFavor_ShouldUpdateAndSave()
    {
        var id = Guid.NewGuid();
        var entity = new FavorEntity { Id = id, Name = "Old", Price = 10m };
        var request = new UpdateFavorDto { Id = id, Name = "New", Price = 25m };
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);

        await Sut().UpdateFavor(request);

        entity.Name.Should().Be("New");
        entity.Price.Should().Be(25m);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task UpdateFavor_ShouldThrowNotFound_AndNotSave_WhenMissing()
    {
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((FavorEntity?)null);

        var act = () => Sut().UpdateFavor(new UpdateFavorDto
        {
            Id = id,
            Name = "New",
            Price = 25m
        });

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Favor {id} not found");
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Never);
    }
}