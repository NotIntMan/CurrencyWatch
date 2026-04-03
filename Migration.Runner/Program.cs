using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Migration.Runner;

using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var logger = loggerFactory.CreateLogger("MigrationRunner");

var factory = new AppDbContextFactory();
using var context = factory.CreateDbContext(args);

logger.LogInformation("Applying migrations...");
context.Database.Migrate();
logger.LogInformation("Migrations applied successfully.");
