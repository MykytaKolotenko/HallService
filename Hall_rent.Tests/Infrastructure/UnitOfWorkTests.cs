using FluentAssertions;
using Hall_rent.Context;
using Hall_rent.Entity;
using Hall_rent.Exceptions.Handling;
using Hall_rent.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hall_rent.Tests.Infrastructure;

public sealed class UnitOfWorkTests
{
    private static (SqliteConnection Connection, AppDbContext Context) CreateSqliteContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return (connection, context);
    }

    private static UnitOfWork CreateUow(AppDbContext context)
    {
        var dispatcher = new ExceptionDispatcher([
            new SerializationConflictResolver(),
            new AppExceptionResolver(),
            new FallbackExceptionResolver()
        ]);
        return new UnitOfWork(context);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var uow = CreateUow(db);
        var hall = new HallEntity { Id = Guid.NewGuid(), Name = "Hall", Persons = 20, Price = 100m };
        db.Halls.Add(hall);

        await uow.SaveChangesAsync();

        (await db.Halls.CountAsync()).Should().Be(1);
    }
}