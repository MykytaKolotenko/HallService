using System.Data;
using FluentAssertions;
using FluentAssertions.Specialized;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Repository.Hall;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Response;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services;

public sealed class BookingServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepository = new Mock<IBookingRepository>();
    private readonly Mock<IFavorRepository> _favorRepository = new Mock<IFavorRepository>();
    private readonly Mock<IHallRepository> _hallRepository = new Mock<IHallRepository>();
    private readonly Mock<IHallUnitOfWork> _unitOfWork = new Mock<IHallUnitOfWork>();

    private BookingService Sut(bool executeTransaction = true)
    {
        if (executeTransaction)
            _unitOfWork
                .Setup(x => x.RunInTransactionAsync(
                    It.IsAny<IsolationLevel>(),
                    It.IsAny<Func<Task<HallBookResponse>>>(),
                    It.IsAny<string>()))
                .Returns<IsolationLevel, Func<Task<HallBookResponse>>, string>(async (_, operation, _) => await operation());

        return new BookingService(
            _hallRepository.Object,
            _bookingRepository.Object,
            _favorRepository.Object,
            _unitOfWork.Object);
    }

    private static BookHallDto Request(Guid hallId, int persons = 10, params Guid[] favorIds)
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

    private static HallEntity Hall(Guid hallId, int capacity = 20, decimal price = 100m, params Guid[] favorIds)
    {
        return new HallEntity
        {
            Id = hallId,
            Name = "Hall",
            Persons = capacity,
            Price = price,
            Favors = favorIds.ToList()
        };
    }

    [Fact]
    public async Task BookAsync_ShouldCreateBookingWithCalculatedPrice()
    {
        Guid hallId = Guid.NewGuid();
        Guid favorId = Guid.NewGuid();
        BookHallDto request = Request(hallId, 10, favorId);
        FavorEntity favor = new FavorEntity { Id = favorId, Name = "Projector", Price = 50m };
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(Hall(hallId, 20, 100m, favorId));
        _bookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(true);
        _favorRepository.Setup(x => x.GetByIdsAsync(It.Is<List<Guid>>(ids => ids.SequenceEqual(new List<Guid> { favorId })))).ReturnsAsync([favor]);
        _bookingRepository.Setup(x => x.AddAsync(It.IsAny<HallBookingEntity>())).Callback<HallBookingEntity>(booking => { booking.Id = Guid.NewGuid(); })
            .Returns(Task.CompletedTask);

        _unitOfWork.Setup(x => x.SaveChangesAsync(default(CancellationToken))).Returns(Task.CompletedTask);

        HallBookResponse result = await Sut().BookAsync(request);

        result.Price.Should().Be(150m);
        result.Id.Should().NotBe(Guid.Empty);
        _bookingRepository.Verify(x => x.AddAsync(It.Is<HallBookingEntity>(b =>
            b.Id != Guid.Empty &&
            b.HallId == hallId &&
            b.StartAt == request.StartAt &&
            b.EndAt == request.EndAt &&
            b.Price == 150m &&
            b.Favors.SequenceEqual(new List<Guid> { favorId }))), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Once);
    }

    [Fact]
    public async Task BookAsync_ShouldUseSerializableTransaction()
    {
        Guid hallId = Guid.NewGuid();
        BookHallDto request = Request(hallId);
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(Hall(hallId));
        _bookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(true);
        _favorRepository.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync([]);
        _bookingRepository.Setup(x => x.AddAsync(It.IsAny<HallBookingEntity>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(x => x.SaveChangesAsync(default(CancellationToken))).Returns(Task.CompletedTask);
        _unitOfWork.Setup(x => x.RunInTransactionAsync(
                IsolationLevel.Serializable,
                It.IsAny<Func<Task<HallBookResponse>>>(),
                $"BookHall({hallId})"))
            .Returns<IsolationLevel, Func<Task<HallBookResponse>>, string>(async (_, operation, _) => await operation());

        await Sut(false).BookAsync(request);

        _unitOfWork.Verify(x => x.RunInTransactionAsync(
            IsolationLevel.Serializable,
            It.IsAny<Func<Task<HallBookResponse>>>(),
            $"BookHall({hallId})"), Times.Once);
    }

    [Fact]
    public async Task BookAsync_ShouldThrowNotFound_WhenHallMissing()
    {
        Guid hallId = Guid.NewGuid();
        BookHallDto request = Request(hallId);
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync((HallEntity?)null);

        Func<Task<HallBookResponse>> act = () => Sut().BookAsync(request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Hall {hallId} not found");
        _bookingRepository.Verify(x => x.AddAsync(It.IsAny<HallBookingEntity>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldThrowCapacityExceeded_WhenRequestedPersonsAreTooMany()
    {
        Guid hallId = Guid.NewGuid();
        BookHallDto request = Request(hallId, 21);
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(Hall(hallId));

        Func<Task<HallBookResponse>> act = () => Sut().BookAsync(request);

        ExceptionAssertions<HallCapacityExceededException> ex = await act.Should().ThrowAsync<HallCapacityExceededException>();
        ex.Which.HallId.Should().Be(hallId);
        ex.Which.Capacity.Should().Be(20);
        ex.Which.Requested.Should().Be(21);
        _bookingRepository.Verify(x => x.IsHallAvailableAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Never);
        _bookingRepository.Verify(x => x.AddAsync(It.IsAny<HallBookingEntity>()), Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldThrowHallNotAvailable_WhenOverlapExists()
    {
        Guid hallId = Guid.NewGuid();
        BookHallDto request = Request(hallId);
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(Hall(hallId));
        _bookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(false);

        Func<Task<HallBookResponse>> act = () => Sut().BookAsync(request);

        ExceptionAssertions<HallNotAvailableException> ex = await act.Should().ThrowAsync<HallNotAvailableException>();
        ex.Which.HallId.Should().Be(hallId);
        _bookingRepository.Verify(x => x.AddAsync(It.IsAny<HallBookingEntity>()), Times.Never);
        _unitOfWork.Verify(x => x.SaveChangesAsync(default(CancellationToken)), Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldThrowFavorsNotOffered_WhenRequestedFavorIsNotOfferedByHall()
    {
        Guid hallId = Guid.NewGuid();
        Guid requestedFavorId = Guid.NewGuid();
        BookHallDto request = Request(hallId, 10, requestedFavorId);
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(Hall(hallId));
        _bookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(true);

        Func<Task<HallBookResponse>> act = () => Sut().BookAsync(request);

        ExceptionAssertions<FavorsNotOfferedException> ex = await act.Should().ThrowAsync<FavorsNotOfferedException>();
        ex.Which.HallId.Should().Be(hallId);
        _favorRepository.Verify(x => x.GetByIdsAsync(It.IsAny<List<Guid>>()), Times.Never);
        _bookingRepository.Verify(x => x.AddAsync(It.IsAny<HallBookingEntity>()), Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldIgnoreDuplicateFavorIds()
    {
        Guid hallId = Guid.NewGuid();
        Guid favorId = Guid.NewGuid();
        BookHallDto request = Request(hallId, 10, favorId, favorId, favorId);
        FavorEntity favor = new FavorEntity { Id = favorId, Name = "Projector", Price = 50m };
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(Hall(hallId, 20, 100m, favorId));
        _bookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(true);
        _favorRepository.Setup(x => x.GetByIdsAsync(It.Is<List<Guid>>(ids => ids.SequenceEqual(new List<Guid> { favorId }))))
            .ReturnsAsync([favor]);
        _bookingRepository.Setup(x => x.AddAsync(It.IsAny<HallBookingEntity>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(x => x.SaveChangesAsync(default(CancellationToken))).Returns(Task.CompletedTask);

        await Sut().BookAsync(request);

        _favorRepository.Verify(x => x.GetByIdsAsync(It.Is<List<Guid>>(ids => ids.Count == 1 && ids[0] == favorId)), Times.Once);
    }

    [Fact]
    public async Task BookAsync_ShouldThrowNotFound_WhenFavorIsMissingFromRepository()
    {
        Guid hallId = Guid.NewGuid();
        Guid favorId = Guid.NewGuid();
        BookHallDto request = Request(hallId, 10, favorId);
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(Hall(hallId, 20, 100m, favorId));
        _bookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(true);
        _favorRepository.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync([]);

        Func<Task<HallBookResponse>> act = () => Sut().BookAsync(request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Favors not found: {favorId}");
        _bookingRepository.Verify(x => x.AddAsync(It.IsAny<HallBookingEntity>()), Times.Never);
    }

    [Fact]
    public async Task BookAsync_ShouldAllowBookingWithoutFavors()
    {
        Guid hallId = Guid.NewGuid();
        BookHallDto request = Request(hallId);
        HallEntity hall = Hall(hallId);
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(hall);
        _bookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(true);
        _favorRepository.Setup(x => x.GetByIdsAsync(It.Is<List<Guid>>(ids => ids.Count == 0))).ReturnsAsync([]);
        _bookingRepository.Setup(x => x.AddAsync(It.IsAny<HallBookingEntity>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(x => x.SaveChangesAsync(default(CancellationToken))).Returns(Task.CompletedTask);

        HallBookResponse result = await Sut().BookAsync(request);

        result.Price.Should().Be(100m);
        _bookingRepository.Verify(x => x.AddAsync(It.Is<HallBookingEntity>(b => b.Favors.Count == 0 && b.Price == 100m)), Times.Once);
    }

    [Fact]
    public async Task BookAsync_ShouldPropagateSaveChangesException()
    {
        Guid hallId = Guid.NewGuid();
        BookHallDto request = Request(hallId);
        _hallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(Hall(hallId));
        _bookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(true);
        _favorRepository.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync([]);
        _bookingRepository.Setup(x => x.AddAsync(It.IsAny<HallBookingEntity>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(x => x.SaveChangesAsync(default(CancellationToken))).ThrowsAsync(new InvalidOperationException("failure"));

        Func<Task<HallBookResponse>> act = () => Sut().BookAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("failure");
    }
}
