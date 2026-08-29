using Hall_rent.Entity;
using Hall_rent.Request;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services.Favor;

public sealed class AddFavourTests : FavorServiceTestBase
{
    [Fact]
    public async Task AddFavour_ShouldAddFavourAndSave()
    {
        var request = new FavorCreateRequest
        {
            Name = "Breakfast",
            Price = 15m
        };

        FavorRepository.Setup(x => x.AddAsync(It.IsAny<FavorEntity>()))
            .Returns(Task.CompletedTask);

        HallUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.AddFavour(request);

        FavorRepository.Verify(x => x.AddAsync(It.Is<FavorEntity>(f =>
            f.Name == request.Name &&
            f.Price == request.Price)), Times.Once);

        HallUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
}
