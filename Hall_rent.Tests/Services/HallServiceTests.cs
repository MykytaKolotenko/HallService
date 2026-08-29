using FluentAssertions;
using FluentAssertions.Specialized;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Repository.Hall;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Service;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services;

public sealed class HallServiceTests
{
    private readonly Mock<IHallRepository> _repository = new Mock<IHallRepository>();
    private readonly Mock<IHallUnitOfWork> _unitOfWork = new Mock<IHallUnitOfWork>();

    private HallService Sut()
    {
        return new HallService(_repository.Object, _unitOfWork.Object);
    }

    [Fact]
    public async Task AddHall_ShouldCreateEntityAndSave()
    {
        Guid favorId = Guid.NewGuid();

        HallCreateDto dto = new HallCreateDto
        {
            Name = "Main Hall",
            Price = 100m,
            Persons = 20,
            Favors = [favorId]
        };

        _repository
            .Setup(x => x.AddAsync(It.IsAny<HallEntity>()))
            .Callback<HallEntity>(hall => { hall.Id = Guid.NewGuid(); })
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default(CancellationToken)))
            .Returns(Task.CompletedTask);

        Guid result = await Sut().AddHall(dto);

        result.Should().NotBe(Guid.Empty);

        _repository.Verify(
            x => x.AddAsync(It.Is<HallEntity>(hall =>
                hall.Id != Guid.Empty &&
                hall.Name == dto.Name &&
                hall.Price == dto.Price &&
                hall.Persons == dto.Persons &&
                hall.Favors.SequenceEqual(dto.Favors!))),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Once);
    }

    [Fact]
    public async Task AddHall_ShouldUseEmptyFavors_WhenNull()
    {
        HallCreateDto dto = new HallCreateDto
        {
            Name = "Main Hall",
            Price = 100m,
            Persons = 20,
            Favors = null
        };

        await Sut().AddHall(dto);

        _repository.Verify(x => x.AddAsync(It.Is<HallEntity>(h => h.Favors.Count == 0)), Times.Once);
    }

    [Fact]
    public async Task AddHall_ShouldTranslateUniqueConstraintException()
    {
        HallCreateDto dto = new HallCreateDto
        {
            Name = "Duplicate",
            Price = 100m,
            Persons = 20,
            Favors = []
        };
        Exception dbException = new Exception("duplicate");
        _unitOfWork.Setup(x => x.SaveChangesAsync(default(CancellationToken)))
            .ThrowsAsync(new UniqueConstraintException("Halls.Name", dbException));

        Func<Task<Guid>> act = () => Sut().AddHall(dto);

        ExceptionAssertions<HallNameAlreadyExistsException> ex = await act.Should().ThrowAsync<HallNameAlreadyExistsException>();
        ex.Which.Name.Should().Be(dto.Name);
        ex.Which.InnerException.Should().BeOfType<UniqueConstraintException>();
    }

    [Fact]
    public async Task UpdateHall_ShouldUpdateAndSave()
    {
        Guid id = Guid.NewGuid();
        HallEntity hall = new HallEntity { Id = id, Name = "Hall", Price = 100m, Persons = 5, Favors = [] };
        Guid favorId = Guid.NewGuid();
        UpdateHallDto request = new UpdateHallDto
        {
            Id = id,
            Price = 200m,
            Persons = 10,
            Favors = [favorId]
        };
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(hall);

        await Sut().UpdateHall(request);

        hall.Price.Should().Be(200m);
        hall.Persons.Should().Be(10);
        hall.Favors.Should().Equal(favorId);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task UpdateHall_ShouldThrowNotFound_AndNotSave_WhenMissing()
    {
        Guid id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((HallEntity?)null);

        Func<Task> act = () => Sut().UpdateHall(new UpdateHallDto
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
        Guid id = Guid.NewGuid();
        HallEntity hall = new HallEntity { Id = id, Name = "Hall", Price = 100m, Persons = 5 };
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(hall);

        await Sut().DeleteHall(id);

        _repository.Verify(x => x.Remove(hall), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task DeleteHall_ShouldThrowNotFound_AndNotRemove_WhenMissing()
    {
        Guid id = Guid.NewGuid();
        _repository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((HallEntity?)null);

        Func<Task> act = () => Sut().DeleteHall(id);

        await act.Should().ThrowAsync<NotFoundException>();
        _repository.Verify(x => x.Remove(It.IsAny<HallEntity>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Never);
    }

    [Fact]
    public async Task FindAvailableHallIdsAsync_ShouldMapIds()
    {
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();
        HallSearchDto request = new HallSearchDto
        {
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Persons = 10
        };
        _repository.Setup(x => x.FindAvailableHallsAsync(request.StartAt, request.EndAt, request.Persons))
            .ReturnsAsync([
                new HallEntity { Id = first },
                new HallEntity { Id = second }
            ]);

        List<Guid> result = await Sut().FindAvailableHallIdsAsync(request);

        result.Should().Equal(first, second);
    }

    [Fact]
    public async Task FindAvailableHallIdsAsync_ShouldThrowNotFound_WhenEmpty()
    {
        HallSearchDto request = new HallSearchDto
        {
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Persons = 10
        };
        _repository.Setup(x => x.FindAvailableHallsAsync(request.StartAt, request.EndAt, request.Persons))
            .ReturnsAsync([]);

        Func<Task<List<Guid>>> act = () => Sut().FindAvailableHallIdsAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}