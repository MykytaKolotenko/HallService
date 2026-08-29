using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Repository;
using Hall_rent.Tests.Support;
using Xunit;

namespace Hall_rent.Tests.Repositories;

public sealed class FavorRepositoryTests
{
    [Fact]
    public async Task AddAsync_AndGetByIdAsync_ShouldPersistFavor()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var repository = new FavorRepository(db);
        var favor = new FavorEntity { Id = Guid.NewGuid(), Name = "Wi-Fi", Price = 10m };

        await repository.AddAsync(favor);
        await db.SaveChangesAsync();

        var result = await repository.GetByIdAsync(favor.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Wi-Fi");
        result.Price.Should().Be(10m);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllFavours()
    {
        await using var db = DbContextFactory.CreateInMemory();
        db.Favours.AddRange(
            new FavorEntity { Id = Guid.NewGuid(), Name = "A", Price = 10m },
            new FavorEntity { Id = Guid.NewGuid(), Name = "B", Price = 20m });
        await db.SaveChangesAsync();
        var result = await new FavorRepository(db).GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnOnlyRequestedIds()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var first = new FavorEntity { Id = Guid.NewGuid(), Name = "A", Price = 10m };
        var second = new FavorEntity { Id = Guid.NewGuid(), Name = "B", Price = 20m };
        var third = new FavorEntity { Id = Guid.NewGuid(), Name = "C", Price = 30m };
        db.Favours.AddRange(first, second, third);
        await db.SaveChangesAsync();

        var result = await new FavorRepository(db).GetByIdsAsync([first.Id, third.Id]);

        result.Select(x => x.Id).Should().BeEquivalentTo([first.Id, third.Id]);
    }

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnEmpty_WhenNoIdsMatch()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var result = await new FavorRepository(db).GetByIdsAsync([Guid.NewGuid()]);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Remove_ShouldDeleteFavor()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var favor = new FavorEntity { Id = Guid.NewGuid(), Name = "A", Price = 10m };
        db.Favours.Add(favor);
        await db.SaveChangesAsync();
        var repository = new FavorRepository(db);

        repository.Remove(favor);
        await db.SaveChangesAsync();

        (await repository.GetByIdAsync(favor.Id)).Should().BeNull();
    }
}