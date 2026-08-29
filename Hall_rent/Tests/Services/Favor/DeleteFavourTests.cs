using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services.Favor;

public sealed class DeleteFavourTests : FavorServiceTestBase
{
    [Fact]
    public async Task DeleteFavour_ShouldRemoveFavourAndSave()
    {
        var id = Guid.NewGuid();

        var favour = new FavorEntity
        {
            Id = id,
            Name = "Wi-Fi",
            Price = 10m
        };

        FavorRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(favour);

        HallUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.DeleteFavour(id);

        FavorRepository.Verify(x => x.Remove(favour), Times.Once);
        HallUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteFavour_ShouldThrowNotFoundException_WhenFavourMissing()
    {
        var id = Guid.NewGuid();

        FavorRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((FavorEntity?)null);

        var sut = CreateSut();

        var act = async () => await sut.DeleteFavour(id);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Favour {id} not found");
    }
}
