using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Tests.Support;
using Xunit;

namespace Hall_rent.Tests.Infrastructure;

public sealed class AppDbContextTests
{
    [Fact]
    public void Model_ShouldContainExpectedEntitiesAndUniqueHallName()
    {
        using var db = DbContextFactory.CreateInMemory();
        var entityTypes = db.Model.GetEntityTypes().Select(x => x.ClrType).ToList();

        entityTypes.Should().Contain(typeof(HallEntity));
        entityTypes.Should().Contain(typeof(FavorEntity));
        entityTypes.Should().Contain(typeof(HallBookingEntity));
        entityTypes.Should().Contain(typeof(HallFavorEntity));

        var hall = db.Model.FindEntityType(typeof(HallEntity));
        hall.Should().NotBeNull();
        hall!.FindProperty(nameof(HallEntity.Name))!.GetMaxLength().Should().Be(255);
        hall.GetIndexes().Should().ContainSingle(i =>
            i.IsUnique && i.Properties.Count == 1 && i.Properties[0].Name == nameof(HallEntity.Name));
    }

    [Fact]
    public async Task ManyToManyJoin_ShouldPersistAndLoadRelationships()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var hall = new HallEntity { Id = Guid.NewGuid(), Name = "Hall", Persons = 20, Price = 100m };
        var favor = new FavorEntity { Id = Guid.NewGuid(), Name = "Projector", Price = 50m };
        var join = new HallFavorEntity { HallId = hall.Id, FavorId = favor.Id, Hall = hall, Favor = favor };
        hall.Favors.Add(join);
        favor.Halls.Add(join);
        db.Halls.Add(hall);
        db.Favors.Add(favor);
        db.Set<HallFavorEntity>().Add(join);
        await db.SaveChangesAsync();

        var exists = db.Set<HallFavorEntity>().Any(x => x.HallId == hall.Id && x.FavorId == favor.Id);
        exists.Should().BeTrue();
    }
}