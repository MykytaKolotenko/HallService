using Hall_rent.Context;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Tests.Support;

public static class DbContextFactory
{
    public static AppDbContext CreateInMemory(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}