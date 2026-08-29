using Ledger.Application;
using Ledger.Application.Accounts;
using Ledger.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Tests;

public class AccountCommandHandlerTests : BaseLedgerTests
{
    private readonly IMediator _mediator;

    public AccountCommandHandlerTests()
    {
        ServiceCollection.AddLedgerApplication(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        _mediator = GetInstance<IMediator>();
    }

    [Fact]
    public async Task Open_Deposit_Withdraw_Produces_Consistent_Balance_And_History()
    {
        var accountId = Guid.NewGuid();

        await _mediator.Send(new OpenAccountCommand(accountId));
        await _mediator.Send(new DepositCommand(accountId, 100m));
        var afterWithdraw = await _mediator.Send(new WithdrawCommand(accountId, 40m));

        Assert.Equal(60m, afterWithdraw.Balance);
        Assert.Equal(3, afterWithdraw.Version);

        var balance = await _mediator.Send(new GetAccountBalanceQuery(accountId));
        Assert.NotNull(balance);
        Assert.Equal(60m, balance!.Balance);

        var history = await _mediator.Send(new GetAccountHistoryQuery(accountId));
        Assert.Equal(new[] { "AccountOpened", "MoneyDeposited", "MoneyWithdrawn" }, history.Select(h => h.EventType));
        Assert.Equal(new long[] { 1, 2, 3 }, history.Select(h => h.Version));
    }

    [Fact]
    public async Task Withdraw_More_Than_Balance_Is_Rejected_And_Does_Not_Persist_New_Event()
    {
        var accountId = Guid.NewGuid();
        await _mediator.Send(new OpenAccountCommand(accountId));
        await _mediator.Send(new DepositCommand(accountId, 50m));

        await Assert.ThrowsAsync<DomainException>(() => _mediator.Send(new WithdrawCommand(accountId, 51m)));

        var history = await _mediator.Send(new GetAccountHistoryQuery(accountId));
        Assert.Equal(2, history.Count); // только AccountOpened + MoneyDeposited

        var balance = await _mediator.Send(new GetAccountBalanceQuery(accountId));
        Assert.Equal(50m, balance!.Balance);
    }

    [Fact]
    public async Task Opening_The_Same_Account_Twice_Is_Rejected()
    {
        var accountId = Guid.NewGuid();
        await _mediator.Send(new OpenAccountCommand(accountId));

        await Assert.ThrowsAsync<DomainException>(() => _mediator.Send(new OpenAccountCommand(accountId)));
    }
}
