using System.Data;
using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Response;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services.Hall;

public sealed class BookHallTests : HallServiceTestBase
{
    [Fact]
    public async Task BookHall_ShouldReturnBookingResponse_WhenSuccess()
    {
        var hallId = Guid.NewGuid();
        var favorId = Guid.NewGuid();

        var request = new BookHallDto
        {
            HallId = hallId,
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Favors = [favorId]
        };

        var hall = new HallEntity
        {
            Id = hallId,
            Persons = 20,
            Price = 100m,
            Favors = [favorId],
            Name = "Hall 1"
        };

        var favor = new FavorEntity
        {
            Id = favorId,
            Name = "Projector",
            Price = 50m
        };

        var expectedPrice = 150m;

        HallUnitOfWork.Setup(x => x.RunInTransactionAsync(
                IsolationLevel.Serializable,
                It.IsAny<Func<Task<HallBookResponse>>>(),
                It.IsAny<string>()))
            .Returns<IsolationLevel, Func<Task<HallBookResponse>>, string>(async (_, op, _) => await op());

        HallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(hall);
        BookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(true);
        FavorRepository.Setup(x => x.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync([favor]);
        BookingRepository.Setup(x => x.AddAsync(It.IsAny<HallBookingEntity>())).Returns(Task.CompletedTask);
        HallUnitOfWork.Setup(x => x.SaveChangesAsync(default)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        var result = await sut.BookHall(request);

        result.Should().NotBeNull();
        result.Price.Should().Be(expectedPrice);
        BookingRepository.Verify(x => x.AddAsync(It.IsAny<HallBookingEntity>()), Times.Once);
        HallUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task BookHall_ShouldThrowNotFoundException_WhenHallMissing()
    {
        var request = new BookHallDto
        {
            HallId = Guid.NewGuid(),
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Favors = []
        };

        HallUnitOfWork.Setup(x => x.RunInTransactionAsync(
                IsolationLevel.Serializable,
                It.IsAny<Func<Task<HallBookResponse>>>(),
                It.IsAny<string>()))
            .Returns<IsolationLevel, Func<Task<HallBookResponse>>, string>(async (_, op, _) => await op());

        HallRepository.Setup(x => x.GetByIdAsync(request.HallId)).ReturnsAsync((HallEntity?)null);

        var sut = CreateSut();

        var act = async () => await sut.BookHall(request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Hall {request.HallId} not found");
    }

    [Fact]
    public async Task BookHall_ShouldThrowHallNotAvailableException_WhenHallIsBooked()
    {
        var hallId = Guid.NewGuid();

        var request = new BookHallDto
        {
            HallId = hallId,
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Favors = []
        };

        var hall = new HallEntity
        {
            Id = hallId,
            Persons = 20,
            Price = 100m,
            Favors = [],
            Name = "Hall 1"
        };

        HallUnitOfWork.Setup(x => x.RunInTransactionAsync(
                IsolationLevel.Serializable,
                It.IsAny<Func<Task<HallBookResponse>>>(),
                It.IsAny<string>()))
            .Returns<IsolationLevel, Func<Task<HallBookResponse>>, string>(async (_, op, _) => await op());

        HallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(hall);
        BookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(false);

        var sut = CreateSut();

        var act = async () => await sut.BookHall(request);

        await act.Should().ThrowAsync<HallNotAvailableException>();
    }

    [Fact]
    public async Task BookHall_ShouldThrowFavoursNotOfferedException_WhenFavorsNotSupported()
    {
        var hallId = Guid.NewGuid();
        var favorId = Guid.NewGuid();

        var request = new BookHallDto
        {
            HallId = hallId,
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Favors = [favorId]
        };

        var hall = new HallEntity
        {
            Id = hallId,
            Persons = 20,
            Price = 100m,
            Favors = [],
            Name = "Hall 1"
        };

        HallUnitOfWork.Setup(x => x.RunInTransactionAsync(
                IsolationLevel.Serializable,
                It.IsAny<Func<Task<HallBookResponse>>>(),
                It.IsAny<string>()))
            .Returns<IsolationLevel, Func<Task<HallBookResponse>>, string>(async (_, op, _) => await op());

        HallRepository.Setup(x => x.GetByIdAsync(hallId)).ReturnsAsync(hall);
        BookingRepository.Setup(x => x.IsHallAvailableAsync(hallId, request.StartAt, request.EndAt)).ReturnsAsync(true);

        var sut = CreateSut();

        var act = async () => await sut.BookHall(request);

        await act.Should().ThrowAsync<FavoursNotOfferedException>();
    }
}
