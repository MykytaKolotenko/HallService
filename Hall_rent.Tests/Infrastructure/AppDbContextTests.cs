using FluentAssertions;
using Hall_rent.Context;
using Hall_rent.Entity;
using Hall_rent.Tests.Support;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace Hall_rent.Tests.Infrastructure;

public sealed class AppDbContextTests
{
    [Fact]
    public void Model_ShouldContainExpectedEntitiesAndUniqueHallName()
    {
        using AppDbContext db = DbContextFactory.CreateInMemory();
        List<Type> entityTypes = db.Model.GetEntityTypes().Select(x => x.ClrType).ToList();

        entityTypes.Should().Contain(typeof(HallEntity));
        entityTypes.Should().Contain(typeof(FavorEntity));
        entityTypes.Should().Contain(typeof(HallBookingEntity));
        entityTypes.Should().Contain(typeof(HallFavorEntity));

        IEntityType? hall = db.Model.FindEntityType(typeof(HallEntity));
        hall.Should().NotBeNull();
        hall!.FindProperty(nameof(HallEntity.Name))!.GetMaxLength().Should().Be(255);
        hall.GetIndexes().Should().ContainSingle(i =>
            i.IsUnique && i.Properties.Count == 1 && i.Properties[0].Name == nameof(HallEntity.Name));
    }

    [Fact]
    public async Task ManyToManyJoin_ShouldPersistAndLoadRelationships()
    {
        await using AppDbContext db = DbContextFactory.CreateInMemory();
        HallEntity hall = new HallEntity { Id = Guid.NewGuid(), Name = "Hall", Persons = 20, Price = 100m };
        FavorEntity favor = new FavorEntity { Id = Guid.NewGuid(), Name = "Projector", Price = 50m };
        HallFavorEntity join = new HallFavorEntity { HallId = hall.Id, FavorId = favor.Id, Hall = hall, Favor = favor };
        hall.FavorsEntity.Add(join);
        favor.Halls.Add(join);
        db.Halls.Add(hall);
        db.Favors.Add(favor);
        db.Set<HallFavorEntity>().Add(join);
        await db.SaveChangesAsync();

        bool exists = db.Set<HallFavorEntity>().Any(x => x.HallId == hall.Id && x.FavorId == favor.Id);
        exists.Should().BeTrue();
    }
}
