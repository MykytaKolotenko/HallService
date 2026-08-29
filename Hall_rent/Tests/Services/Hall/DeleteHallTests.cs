using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services.Hall;

public sealed class DeleteHallTests : HallServiceTestBase
{
    [Fact]
    public async Task DeleteHall_ShouldRemoveAndSave()
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

        HallRepository.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(hall);
        HallUnitOfWork.Setup(x => x.SaveChangesAsync(default)).Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.DeleteHall(id);

        HallRepository.Verify(x => x.Remove(hall), Times.Once);
        HallUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteHall_ShouldThrowNotFoundException_WhenHallMissing()
    {
        var id = Guid.NewGuid();

        HallRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((HallEntity?)null);

        var sut = CreateSut();

        var act = async () => await sut.DeleteHall(id);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Hall {id} not found");
    }
}
