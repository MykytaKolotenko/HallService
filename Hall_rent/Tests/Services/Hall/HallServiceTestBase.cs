using Hall_rent.Repository.Hall;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Service;
using Moq;

namespace Hall_rent.Tests.Services.Hall;

public abstract class HallServiceTestBase
{
    protected readonly Mock<IBookingRepository> BookingRepository = new();
    protected readonly Mock<IFavorRepository> FavorRepository = new();
    protected readonly Mock<IHallRepository> HallRepository = new();
    protected readonly Mock<IHallUnitOfWork> HallUnitOfWork = new();
    protected readonly Mock<ILogger<HallService>> Logger = new();

    protected HallService CreateSut()
        => new HallService(
            HallRepository.Object,
            HallUnitOfWork.Object,
            BookingRepository.Object,
            FavorRepository.Object,
            Logger.Object);
}
