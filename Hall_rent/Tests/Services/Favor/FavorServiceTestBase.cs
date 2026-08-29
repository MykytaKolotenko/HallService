using Hall_rent.Repository.Interfaces;
using Hall_rent.Service;
using Moq;

namespace Hall_rent.Tests.Services.Favor;

public abstract class FavorServiceTestBase
{
    protected readonly Mock<IFavorRepository> FavorRepository = new();
    protected readonly Mock<IHallUnitOfWork> HallUnitOfWork = new();

    protected FavorService CreateSut()
        => new FavorService(
            FavorRepository.Object,
            HallUnitOfWork.Object);
}
