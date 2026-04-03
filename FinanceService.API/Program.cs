using Common.API.Extensions;
using Common.Database;
using FinanceService.API.Exceptions;
using FinanceService.Application.Queries.GetAllCurrencies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<AppExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddJwtBearerWithBlacklist(builder.Configuration);

builder.Services.AddMediatR(
    cfg => cfg.RegisterServicesFromAssemblyContaining<GetAllCurrenciesQuery>()
);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerWithBearer();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
