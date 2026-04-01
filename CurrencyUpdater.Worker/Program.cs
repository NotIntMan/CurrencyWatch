using System.Text;
using Common.Database;
using Common.Infrastructure;
using CurrencyUpdater.Worker;
using CurrencyUpdater.Worker.Commands;
using Microsoft.EntityFrameworkCore;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var builder = Host.CreateApplicationBuilder(args);

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? throw new InvalidOperationException("CONNECTION_STRING environment variable is not set.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<CbrXmlParser>();
builder.Services.AddHttpClient<UpdateCurrenciesCommandHandler>();
builder.Services.AddHostedService<CurrencyUpdateService>();

var host = builder.Build();
host.Run();
