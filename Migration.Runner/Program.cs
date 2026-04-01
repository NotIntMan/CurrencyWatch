using Migration.Runner;
using Microsoft.EntityFrameworkCore;

var factory = new AppDbContextFactory();
using var context = factory.CreateDbContext(args);

Console.WriteLine("Applying migrations...");
context.Database.Migrate();
Console.WriteLine("Migrations applied successfully.");
