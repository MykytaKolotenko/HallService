using Hall_rent.Dto;
using Hall_rent.Entity;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services.Hall;

public sealed class AddHallTests : HallServiceTestBase
{
    [Fact]
    public async Task AddHall_ShouldAddHall()
    {
        var dto = new HallCreateDto
        {
            Persons = 10,
            Price = 100m,
            Favors = [],
            Name = "Hall 1"
        };

        HallUnitOfWork.Setup(x => x.SaveChangesAsync(default))
            .Returns(Task.CompletedTask);

        var sut = CreateSut();

        await sut.AddHall(dto);

        HallRepository.Verify(x => x.AddAsync(It.Is<HallEntity>(h =>
            h.Persons == dto.Persons &&
            h.Price == dto.Price &&
            h.Name == dto.Name &&
            h.Favors.SequenceEqual(dto.Favors))), Times.Once);

        HallUnitOfWork.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }
}
