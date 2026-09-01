using FluentAssertions;
using Hall_rent.Entity;
using Hall_rent.Exceptions;
using Hall_rent.Exceptions.Handling;
using Hall_rent.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hall_rent.Tests.Exceptions;

public sealed class UniqueConstraintExceptionFactoryTests
{
    [Fact]
    public async Task Create_ShouldReturnHallNameAlreadyExists_WhenFailedEntityIsHall()
    {
        await using var db = DbContextFactory.CreateInMemory();
        var hall = new HallEntity { Id = Guid.NewGuid(), Name = "Sunrise Room", Persons = 10, Price = 50m };
        db.Attach(hall);
        var entry = db.Entry(hall);

        var dbUpdateEx = new DbUpdateException("duplicate key", new Exception("inner"), [entry]);

        var result = UniqueConstraintExceptionFactory.Create(dbUpdateEx);

        result.Should().BeOfType<HallNameAlreadyExistsException>();
        result.Message.Should().Be("Hall with name 'Sunrise Room' already exists.");
    }

    [Fact]
    public void Create_ShouldReturnGenericUniqueConstraintException_WhenEntityTypeUnknown()
    {
        var result = UniqueConstraintExceptionFactory.Create(
            new DbUpdateException("duplicate key", new Exception("inner")));

        result.Should().BeOfType<UniqueConstraintException>();
    }
}