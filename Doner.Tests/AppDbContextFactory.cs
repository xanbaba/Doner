using Doner.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Doner.Tests;

public class AppDbContextFactory : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
        return new AppDbContext(options);
    }
}