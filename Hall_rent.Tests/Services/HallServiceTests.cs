using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Service;
using Hall_rent.Service.Interface;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services;

public sealed class HallServiceTests
{
    private readonly Mock<IFavorResolver> _favorResolver = new Mock<IFavorResolver>();
    private readonly Mock<IHallRepository> _repository = new Mock<IHallRepository>();
    private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();

    private HallService Sut()
    {
        return new HallService(_repository.Object, _unitOfWork.Object, _favorResolver.Object);
    }

    [Fact]
    public async Task AddHall_ShouldCreateEntityAndSave()
    {
        var favorId = Guid.NewGuid();
        var favor = new FavorEntity { Id = favorId, Name = "Projector", Price = 50m };

        var dto = new HallCreateDto
        {
            Name = "Main Hall",
            Price = 100m,
            Persons = 20,
            Favors = [favorId]
        };

        _favorResolver
            .Setup(x => x.ResolveOrThrowAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([favor]);

        HallEntity? addedHall = null;

        _repository
            .Setup(x => x.AddAsync(It.IsAny<HallEntity>()))
            .Callback<HallEntity>(hall =>
            {
                addedHall = hall;
                hall.Id = Guid.NewGuid();
            })
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default(CancellationToken)))
            .Returns(Task.CompletedTask);

        var result = await Sut().CreateHall(dto);

        result.Should().NotBe(Guid.Empty);
        addedHall.Should().NotBeNull();
        addedHall!.Id.Should().NotBe(Guid.Empty);

        _repository.Verify(
            x => x.AddAsync(It.Is<HallEntity>(hall =>
                hall.Name == dto.Name &&
                hall.Price == dto.Price &&
                hall.Persons == dto.Persons &&
                hall.Favors.Select(f => f.FavorId).SequenceEqual(new[] { favorId }))),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Once);
    }

    [Fact]
    public async Task AddHall_ShouldUseEmptyFavors_WhenNull()
    {
        var dto = new HallCreateDto
        {
            Name = "Main Hall",
            Price = 100m,
            Persons = 20,
            Favors = null
        };

        _favorResolver
            .Setup(x => x.ResolveOrThrowAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([]);

        await Sut().CreateHall(dto);

        _repository.Verify(x => x.AddAsync(It.Is<HallEntity>(h => h.Favors.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task UpdateHall_ShouldUpdateAndSave()
    {
        var id = Guid.NewGuid();
        var hall = new HallEntity { Id = id, Name = "Hall", Price = 100m, Persons = 5, Favors = [] };
        var favorId = Guid.NewGuid();
        var favor = new FavorEntity { Id = favorId, Name = "Catering", Price = 30m };
        var request = new UpdateHallDto
        {
            Id = id,
            Price = 200m,
            Persons = 10,
            Favors = [favorId]
        };
        _repository.Setup(x => x.GetByIdWithFavorsAsync(id)).ReturnsAsync(hall);
        _favorResolver
            .Setup(x => x.ResolveOrThrowAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync([favor]);

        await Sut().UpdateHall(request);

        hall.Price.Should().Be(200m);
        hall.Persons.Should().Be(10);
        hall.Favors.Select(f => f.FavorId).Should().Equal(favorId);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task UpdateHall_ShouldThrowNotFound_AndNotSave_WhenMissing()
    {
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdWithFavorsAsync(id)).ReturnsAsync((HallEntity?)null);

        var act = () => Sut().UpdateHall(new UpdateHallDto
        {
            Id = id,
            Price = 200m,
            Persons = 10,
            Favors = []
        });

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Hall {id} not found");
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Never);
    }

    [Fact]
    public async Task DeleteHall_ShouldRemoveAndSave()
    {
        var id = Guid.NewGuid();
        var hall = new HallEntity { Id = id, Name = "Hall", Price = 100m, Persons = 5 };
        _repository.Setup(x => x.GetByIdWithFavorsAsync(id)).ReturnsAsync(hall);

        await Sut().DeleteHall(id);

        _repository.Verify(x => x.Remove(hall), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task DeleteHall_ShouldThrowNotFound_AndNotRemove_WhenMissing()
    {
        var id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdWithFavorsAsync(id)).ReturnsAsync((HallEntity?)null);

        var act = () => Sut().DeleteHall(id);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(x => x.Remove(It.IsAny<HallEntity>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Never);
    }

    [Fact]
    public async Task FindAvailableHallIdsAsync_ShouldMapIds()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var request = new HallSearchDto
        {
            From = DateTime.UtcNow.AddDays(1),
            To = DateTime.UtcNow.AddDays(1).AddHours(2),
            Persons = 10
        };
        _repository.Setup(x => x.FindAvailableHallsAsync(request.From, request.To, request.Persons))
            .ReturnsAsync([
                new HallEntity { Id = first },
                new HallEntity { Id = second }
            ]);

        var result = await Sut().SearchAvailableHallIdsAsync(request);

        result.Halls.Should().Equal(first, second);
    }

    [Fact]
    public async Task FindAvailableHallIdsAsync_ShouldThrowNotFound_WhenEmpty()
    {
        var request = new HallSearchDto
        {
            From = DateTime.UtcNow.AddDays(1),
            To = DateTime.UtcNow.AddDays(1).AddHours(2),
            Persons = 10
        };
        _repository.Setup(x => x.FindAvailableHallsAsync(request.From, request.To, request.Persons))
            .ReturnsAsync([]);

        var act = () => Sut().SearchAvailableHallIdsAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}