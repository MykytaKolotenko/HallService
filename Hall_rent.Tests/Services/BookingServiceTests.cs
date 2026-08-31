using System.Data;
using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Response;
using Hall_rent.Service;
using Hall_rent.Service.Interface;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services;

public sealed class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new Mock<IBookingRepository>();
    private readonly Mock<IFavorResolver> _favorResolver = new Mock<IFavorResolver>();
    private readonly Mock<IHallRepository> _hallRepository = new Mock<IHallRepository>();
    private readonly Mock<ITransactionRunner> _transactionRunner = new Mock<ITransactionRunner>();
    private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();

    private BookingService Sut(bool executeTransaction = true)
    {
        if (executeTransaction)
            _transactionRunner
                .Setup(x => x.RunInTransactionAsync(
                    It.IsAny<IsolationLevel>(),
                    It.IsAny<Func<Task<HallBookResponse>>>()))
                .Returns<IsolationLevel, Func<Task<HallBookResponse>>, string>(async (_, operation, _) => await operation());

        return new BookingService(
            _hallRepository.Object,
            _bookingRepository.Object,
            _unitOfWork.Object,
            _favorResolver.Object,
            _transactionRunner.Object);
    }

    private static BookHallDto Request(
        Guid hallId,
        int persons = 10,
        params Guid[] favorIds)
    {
        return new BookHallDto
        {
            HallId = hallId,
            Persons = persons,
            StartAt = DateTime.UtcNow.AddDays(2),
            EndAt = DateTime.UtcNow.AddDays(2).AddHours(2),
            Favors = favorIds.ToList()
        };
    }

    private static HallEntity Hall(
        Guid hallId,
        int capacity = 20,
        decimal price = 100m,
        params Guid[] favorIds)
    {
        return new HallEntity
        {
            Id = hallId,
            Name = "Hall",
            Persons = capacity,
            Price = price,
            Favors = favorIds
                .Select(id => new HallFavorEntity
                {
                    HallId = hallId,
                    FavorId = id
                })
                .ToList()
        };
    }

    [Fact]
    public async Task BookAsync_ShouldCreateBookingWithCalculatedPrice()
    {
        var hallId = Guid.NewGuid();
        var favorId = Guid.NewGuid();

        var request = Request(hallId, 10, favorId);

        var favor = new FavorEntity
        {
            Id = favorId,
            Name = "Projector",
            Price = 50m
        };

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync(Hall(hallId, 20, 100m, favorId));

        _bookingRepository
            .Setup(x => x.IsHallAvailableAsync(
                hallId,
                request.StartAt,
                request.EndAt))
            .ReturnsAsync(true);

        _favorResolver
            .Setup(x => x.ResolveOrThrowAsync(
                It.Is<List<Guid>>(ids =>
                    ids.SequenceEqual(new List<Guid> { favorId }))))
            .ReturnsAsync(new List<FavorEntity> { favor });

        _bookingRepository
            .Setup(x => x.AddAsync(It.IsAny<HallBookingEntity>()))
            .Callback<HallBookingEntity>(booking => { booking.Id = Guid.NewGuid(); })
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default(CancellationToken)))
            .Returns(Task.CompletedTask);

        var result = await Sut().BookAsync(request);

        result.Price.Should().Be(150m);
        result.Id.Should().NotBe(Guid.Empty);

        _bookingRepository.Verify(
            x => x.AddAsync(It.Is<HallBookingEntity>(b =>
                b.Id != Guid.Empty &&
                b.HallId == hallId &&
                b.From == request.StartAt &&
                b.To == request.EndAt &&
                b.Price == 150m &&
                b.Favors.Count == 1 &&
                b.Favors.Any(f => f.FavorId == favorId))),
            Times.Once);

        _favorResolver.Verify(
            x => x.ResolveOrThrowAsync(
                It.Is<List<Guid>>(ids =>
                    ids.SequenceEqual(new List<Guid> { favorId }))),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Once);
    }

    [Fact]
    public async Task BookAsync_ShouldUseSerializableTransaction()
    {
        var hallId = Guid.NewGuid();
        var request = Request(hallId);

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync(Hall(hallId));

        _bookingRepository
            .Setup(x => x.IsHallAvailableAsync(
                hallId,
                request.StartAt,
                request.EndAt))
            .ReturnsAsync(true);

        _favorResolver
            .Setup(x => x.ResolveOrThrowAsync(
                It.Is<List<Guid>>(ids => ids.Count == 0)))
            .ReturnsAsync(new List<FavorEntity>());

        _bookingRepository
            .Setup(x => x.AddAsync(It.IsAny<HallBookingEntity>()))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default(CancellationToken)))
            .Returns(Task.CompletedTask);

        _transactionRunner
            .Setup(x => x.RunInTransactionAsync(
                IsolationLevel.Serializable,
                It.IsAny<Func<Task<HallBookResponse>>>()))
            .Returns<IsolationLevel, Func<Task<HallBookResponse>>, string>(async (_, operation, _) => await operation());

        await Sut(false).BookAsync(request);

        _transactionRunner.Verify(
            x => x.RunInTransactionAsync(
                IsolationLevel.Serializable,
                It.IsAny<Func<Task<HallBookResponse>>>()),
            Times.Once);
    }

    [Fact]
    public async Task BookAsync_ShouldThrowNotFound_WhenHallMissing()
    {
        var hallId = Guid.NewGuid();
        var request = Request(hallId);

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync((HallEntity?)null);

        var act = () => Sut().BookAsync(request);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Hall {hallId} not found");

        _bookingRepository.Verify(
            x => x.IsHallAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()),
            Times.Never);

        _favorResolver.Verify(
            x => x.ResolveOrThrowAsync(
                It.IsAny<List<Guid>>()),
            Times.Never);

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<HallBookingEntity>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldThrowCapacityExceeded_WhenRequestedPersonsAreTooMany()
    {
        var hallId = Guid.NewGuid();
        var request = Request(hallId, 21);

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync(Hall(hallId, 20));

        var act = () => Sut().BookAsync(request);

        var ex = await act
            .Should()
            .ThrowAsync<HallCapacityExceededException>();

        ex.Which.HallId.Should().Be(hallId);
        ex.Which.Capacity.Should().Be(20);
        ex.Which.Requested.Should().Be(21);

        _bookingRepository.Verify(
            x => x.IsHallAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>()),
            Times.Never);

        _favorResolver.Verify(
            x => x.ResolveOrThrowAsync(
                It.IsAny<List<Guid>>()),
            Times.Never);

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<HallBookingEntity>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldThrowHallNotAvailable_WhenOverlapExists()
    {
        var hallId = Guid.NewGuid();
        var request = Request(hallId);

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync(Hall(hallId));

        _bookingRepository
            .Setup(x => x.IsHallAvailableAsync(
                hallId,
                request.StartAt,
                request.EndAt))
            .ReturnsAsync(false);

        var act = () => Sut().BookAsync(request);

        var ex = await act
            .Should()
            .ThrowAsync<HallNotAvailableException>();

        ex.Which.HallId.Should().Be(hallId);

        _favorResolver.Verify(
            x => x.ResolveOrThrowAsync(
                It.IsAny<List<Guid>>()),
            Times.Never);

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<HallBookingEntity>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldThrowFavorsNotOffered_WhenRequestedFavorIsNotOfferedByHall()
    {
        var hallId = Guid.NewGuid();
        var requestedFavorId = Guid.NewGuid();

        var request = Request(
            hallId,
            10,
            requestedFavorId);

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync(Hall(hallId));

        _bookingRepository
            .Setup(x => x.IsHallAvailableAsync(
                hallId,
                request.StartAt,
                request.EndAt))
            .ReturnsAsync(true);

        var act = () => Sut().BookAsync(request);

        var ex = await act
            .Should()
            .ThrowAsync<FavorsNotOfferedException>();

        ex.Which.HallId.Should().Be(hallId);

        _favorResolver.Verify(
            x => x.ResolveOrThrowAsync(
                It.IsAny<IEnumerable<Guid>>()),
            Times.Never);

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<HallBookingEntity>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldIgnoreDuplicateFavorIds()
    {
        var hallId = Guid.NewGuid();
        var favorId = Guid.NewGuid();

        var request = Request(
            hallId,
            10,
            favorId,
            favorId,
            favorId);

        var favor = new FavorEntity
        {
            Id = favorId,
            Name = "Projector",
            Price = 50m
        };

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync(Hall(hallId, 20, 100m, favorId));

        _bookingRepository
            .Setup(x => x.IsHallAvailableAsync(
                hallId,
                request.StartAt,
                request.EndAt))
            .ReturnsAsync(true);

        _favorResolver
            .Setup(x => x.ResolveOrThrowAsync(
                It.Is<List<Guid>>(ids =>
                    ids.SequenceEqual(new List<Guid> { favorId }))))
            .ReturnsAsync(new List<FavorEntity> { favor });

        _bookingRepository
            .Setup(x => x.AddAsync(
                It.IsAny<HallBookingEntity>()))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default(CancellationToken)))
            .Returns(Task.CompletedTask);

        await Sut().BookAsync(request);

        _favorResolver.Verify(
            x => x.ResolveOrThrowAsync(
                It.Is<List<Guid>>(ids =>
                    ids.Count == 1 &&
                    ids[0] == favorId)),
            Times.Once);

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.Is<HallBookingEntity>(b =>
                    b.Favors.Count == 1 &&
                    b.Favors.Any(f => f.FavorId == favorId))),
            Times.Once);
    }

    [Fact]
    public async Task BookAsync_ShouldPropagateNotFound_WhenFavorCannotBeResolved()
    {
        var hallId = Guid.NewGuid();
        var favorId = Guid.NewGuid();

        var request = Request(
            hallId,
            10,
            favorId);

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync(
                Hall(hallId, 20, 100m, favorId));

        _bookingRepository
            .Setup(x => x.IsHallAvailableAsync(
                hallId,
                request.StartAt,
                request.EndAt))
            .ReturnsAsync(true);

        _favorResolver
            .Setup(x => x.ResolveOrThrowAsync(
                It.Is<List<Guid>>(ids =>
                    ids.SequenceEqual(new List<Guid> { favorId }))))
            .ThrowsAsync(
                new NotFoundException(
                    $"Favors not found: {favorId}"));

        var act = () => Sut().BookAsync(request);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"Favors not found: {favorId}");

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<HallBookingEntity>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldAllowBookingWithoutFavors()
    {
        var hallId = Guid.NewGuid();
        var request = Request(hallId);

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync(Hall(hallId));

        _bookingRepository
            .Setup(x => x.IsHallAvailableAsync(
                hallId,
                request.StartAt,
                request.EndAt))
            .ReturnsAsync(true);

        _favorResolver
            .Setup(x => x.ResolveOrThrowAsync(
                It.Is<IEnumerable<Guid>>(ids => !ids.Any())))
            .ReturnsAsync(new List<FavorEntity>());

        _bookingRepository
            .Setup(x => x.AddAsync(
                It.IsAny<HallBookingEntity>()))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default(CancellationToken)))
            .Returns(Task.CompletedTask);

        var result = await Sut().BookAsync(request);

        result.Price.Should().Be(100m);

        _favorResolver.Verify(
            x => x.ResolveOrThrowAsync(
                It.Is<IEnumerable<Guid>>(ids => !ids.Any())),
            Times.Once);

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.Is<HallBookingEntity>(b =>
                    b.Favors.Count == 0 &&
                    b.Price == 100m)),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Once);
    }

    [Fact]
    public async Task BookAsync_ShouldPropagateSaveChangesException()
    {
        var hallId = Guid.NewGuid();
        var request = Request(hallId);

        _hallRepository
            .Setup(x => x.GetByIdWithFavorsAsync(hallId))
            .ReturnsAsync(Hall(hallId));

        _bookingRepository
            .Setup(x => x.IsHallAvailableAsync(
                hallId,
                request.StartAt,
                request.EndAt))
            .ReturnsAsync(true);

        _favorResolver
            .Setup(x => x.ResolveOrThrowAsync(
                It.Is<List<Guid>>(ids => ids.Count == 0)))
            .ReturnsAsync(new List<FavorEntity>());

        _bookingRepository
            .Setup(x => x.AddAsync(
                It.IsAny<HallBookingEntity>()))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync(default(CancellationToken)))
            .ThrowsAsync(
                new InvalidOperationException("failure"));

        var act = () => Sut().BookAsync(request);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("failure");

        _bookingRepository.Verify(
            x => x.AddAsync(
                It.IsAny<HallBookingEntity>()),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(default(CancellationToken)),
            Times.Once);
    }
}
