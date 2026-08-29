using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services.Hall;

public sealed class UpdateHallTests : HallServiceTestBase
{
    [Fact]
    public async Task UpdateHall_ShouldUpdateHallAndSave()
    {
        var id = Guid.NewGuid();

        var hall = new HallEntity
        {
            Id = id,
            Persons = 1,
            Price = 100m,
            Favors = [],
            Name = "Hall 1"
        };

        var request = new UpdateHallDto
        {
            Id = id,
            Price = 200m,
            Persons = 20,
            Favors = [Guid.NewGuid()]
        };

        HallRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(hall);
        HallUnitOfWork.Setup(x => x.SaveChangesAsync(default)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.UpdateHall(request);

        hall.Persons.Should().Be(20);
        hall.Price.Should().Be(200m);
        hall.Favors.Should().BeEquivalentTo(request.Favors);
        HallUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateHall_ShouldThrowNotFoundException_WhenHallMissing()
    {
        var id = Guid.NewGuid();

        var request = new UpdateHallDto
        {
            Id = id,
            Price = 200m,
            Persons = 20,
            Favors = []
        };

        HallRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((HallEntity?)null);

        var sut = CreateSut();

        var act = async () => await sut.UpdateHall(request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Hall {id} not found");
    }
}
