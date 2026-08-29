using Ledger.Application;
using Ledger.Application.Accounts;
using Ledger.Application.Persistence;
using Ledger.Domain;
using Ledger.EventStore;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLedgerApplication(builder.Configuration.GetConnectionString("Ledger")!);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<LedgerDbContext>().Database.EnsureCreated();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (DomainException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (EventStreamConcurrencyException ex)
    {
        context.Response.StatusCode = StatusCodes.Status409Conflict;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

app.MapGet("/health/db", async (LedgerDbContext db) =>
    await db.Database.CanConnectAsync()
        ? Results.Ok("db: ok")
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.MapPost("/accounts", async (IMediator mediator) =>
{
    var result = await mediator.Send(new OpenAccountCommand(Guid.NewGuid()));
    return Results.Created($"/accounts/{result.AccountId}/balance", result);
});

app.MapPost("/accounts/{accountId:guid}/deposits", async (Guid accountId, DepositRequest body, IMediator mediator) =>
    Results.Ok(await mediator.Send(new DepositCommand(accountId, body.Amount))));

app.MapPost("/accounts/{accountId:guid}/withdrawals", async (Guid accountId, WithdrawRequest body, IMediator mediator) =>
    Results.Ok(await mediator.Send(new WithdrawCommand(accountId, body.Amount))));

app.MapGet("/accounts/{accountId:guid}/balance", async (Guid accountId, IMediator mediator) =>
{
    var balance = await mediator.Send(new GetAccountBalanceQuery(accountId));
    return balance is null ? Results.NotFound() : Results.Ok(balance);
});

app.MapGet("/accounts/{accountId:guid}/history", async (Guid accountId, IMediator mediator) =>
    Results.Ok(await mediator.Send(new GetAccountHistoryQuery(accountId))));

app.Run();

record DepositRequest(decimal Amount);
record WithdrawRequest(decimal Amount);
