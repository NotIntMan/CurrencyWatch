using Common.Database;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UserService.Application.Tests.TestHelpers;

public static class TestDbContext
{
    public static AppDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
