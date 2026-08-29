using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services.Favor;

public sealed class UpdateFavourTests : FavorServiceTestBase
{
    [Fact]
    public async Task UpdateFavour_ShouldUpdateFavourAndSave()
    {
        var id = Guid.NewGuid();

        var favour = new FavorEntity
        {
            Id = id,
            Name = "Old name",
            Price = 100m
        };

        var request = new UpdateFavorDto
        {
            Id = id,
            Name = "New name",
            Price = 200m
        };

        FavorRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(favour);

        HallUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.UpdateFavour(request);

        favour.Name.Should().Be("New name");
        favour.Price.Should().Be(200m);

        HallUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateFavour_ShouldThrowNotFoundException_WhenFavourMissing()
    {
        var id = Guid.NewGuid();

        var request = new UpdateFavorDto
        {
            Id = id,
            Name = "New name",
            Price = 200m
        };

        FavorRepository.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync((FavorEntity?)null);

        var sut = CreateSut();

        var act = async () => await sut.UpdateFavour(request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Favour {id} not found");
    }
}
