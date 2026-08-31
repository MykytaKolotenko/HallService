using System.Data;
using FluentAssertions;
using Hall_rent.Context;
using Hall_rent.Entity;
using Hall_rent.Exceptions.Handling;
using Hall_rent.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Hall_rent.Tests.Infrastructure;

public class TransactionRunnerTests
{
    private static TransactionRunner CreateUow(AppDbContext context)
    {
        var dispatcher = new ExceptionDispatcher([
            new SerializationConflictResolver(),
            new AppExceptionResolver(),
            new FallbackExceptionResolver()
        ]);
        return new TransactionRunner(context);
    }

    [Fact]
    public async Task RunInTransactionAsync_ShouldCommit_WhenOperationSucceeds()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var uow = CreateUow(db);
        var hall = new HallEntity { Id = Guid.NewGuid(), Name = "Committed", Persons = 10, Price = 50m };

        var result = await uow.RunInTransactionAsync(
            IsolationLevel.Serializable,
            async () =>
            {
                db.Halls.Add(hall);
                await db.SaveChangesAsync();
                return hall.Id;
            });

        result.Should().Be(hall.Id);
        await using var verification = new AppDbContext(options);
        (await verification.Halls.AnyAsync(x => x.Id == hall.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task RunInTransactionAsync_ShouldRollback_WhenOperationThrows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var uow = CreateUow(db);
        var hall = new HallEntity { Id = Guid.NewGuid(), Name = "RolledBack", Persons = 10, Price = 50m };

        async Task<bool> Operation()
        {
            db.Halls.Add(hall);
            await db.SaveChangesAsync();

            throw new InvalidOperationException("boom");
        }

        var act = () => uow.RunInTransactionAsync(
            IsolationLevel.Serializable,
            Operation);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("boom");
        await using var verification = new AppDbContext(options);
        (await verification.Halls.AnyAsync(x => x.Id == hall.Id)).Should().BeFalse();
    }
}