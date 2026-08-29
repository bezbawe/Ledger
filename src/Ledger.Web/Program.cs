using Ledger.Application;
using Ledger.EventStore;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LedgerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Ledger")));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));

var app = builder.Build();

// Каркас: связность с БД и прохождение команды через MediatR.
app.MapGet("/health/db", async (LedgerDbContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok("db: ok")
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.MapGet("/ping", async (IMediator mediator) =>
    Results.Ok(await mediator.Send(new Ping("scaffold"))));

app.Run();
