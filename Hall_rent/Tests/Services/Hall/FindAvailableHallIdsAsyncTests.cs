using FluentAssertions;
using Hall_rent.Dto;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Moq;
using Xunit;

namespace Hall_rent.Tests.Services.Hall;

public sealed class FindAvailableHallIdsAsyncTests : HallServiceTestBase
{
    [Fact]
    public async Task FindAvailableHallIdsAsync_ShouldReturnIds()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var request = new HallSearchDto
        {
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Persons = 10
        };

        HallRepository.Setup(x => x.FindAvailableHallsAsync(request.StartAt, request.EndAt, request.Persons))
            .ReturnsAsync(new List<HallEntity>
            {
                new HallEntity { Id = id1 },
                new HallEntity { Id = id2 }
            });

        var sut = CreateSut();

        var result = await sut.FindAvailableHallIdsAsync(request);

        result.Should().BeEquivalentTo([id1, id2]);
    }

    [Fact]
    public async Task FindAvailableHallIdsAsync_ShouldThrowNotFoundException_WhenNoHalls()
    {
        var request = new HallSearchDto
        {
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(2),
            Persons = 10
        };

        HallRepository.Setup(x => x.FindAvailableHallsAsync(request.StartAt, request.EndAt, request.Persons))
            .ReturnsAsync([]);

        var sut = CreateSut();

        var act = async () => await sut.FindAvailableHallIdsAsync(request);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"No halls available for {request.Persons} persons from {request.StartAt:yyyy-MM-dd HH:mm} to {request.EndAt:yyyy-MM-dd HH:mm}.");
    }
}
